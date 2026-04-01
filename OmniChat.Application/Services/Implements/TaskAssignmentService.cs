using Amazon.Runtime.Internal;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Intent;
using OmniChat.Infrastructure.Dtos.Responses.Intent;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Net.Http.Json;

namespace OmniChat.Application.Services.Implements;

public class TaskAssignmentService : BaseService<TaskAssignmentService>, ITaskAssignmentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private static readonly SemaphoreSlim _assignmentLock = new SemaphoreSlim(1, 1);
    public TaskAssignmentService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<TaskAssignmentService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IConfiguration config) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public async Task ProcessTask(PredictRequest predictRequest, Guid conversationId)
    {
        var predict = await AnalyzeAsync(predictRequest);
        if (predict == null)
        {
            _logger.LogWarning("Intent analysis failed for conversation {ConversationId}", conversationId);
            return;
        }

        var isSuccessfullyCreatingTask = await CreateTaskAsync(predict, conversationId);
        if (!isSuccessfullyCreatingTask) return;

        var assigned = await AssignStaffToConversationAsync(conversationId);

        if (!assigned)
        {
            await ProcessWaitingQueueAsync();
        }
    }

    public async Task ProcessWaitingQueueAsync()
    {
        if (!await _assignmentLock.WaitAsync(0)) return;

        try
        {
            var conversationRepo = _unitOfWork.GetRepository<SupportConversation>();

            while (true)
            {
                var nextConversation = await conversationRepo.SingleOrDefaultAsync(
                    predicate: c => !c.IsDistributed && c.Status == ConversationStatus.Pending,
                    orderBy: q => q.OrderBy(c => c.CreatedDate)
                );

                if (nextConversation == null) break;

                bool assigned = await AssignStaffToConversationAsync(nextConversation.Id);

                if (!assigned) break;
            }
        }
        finally
        {
            _assignmentLock.Release();
        }
    }

    private async Task<bool> CreateTaskAsync(PredictResponse predictResponse, Guid conversationId)
    {
        if (predictResponse?.Details == null)
            return false;

        var predictedLabels = predictResponse.Details
            .Where(x => x.Predicted)
            .ToList();

        if (!predictedLabels.Any())
            return false;

        var intentTypesRepo = _unitOfWork.GetRepository<IntentType>();
        var taskRepo = _unitOfWork.GetRepository<SupportTask>();

        var labels = predictedLabels.Select(x => x.Label).ToList();

        var intentTypes = await intentTypesRepo.GetListAsync(
            predicate: x => labels.Contains(x.TypeName)
        );

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var tasks = predictedLabels
                .Join(intentTypes,
                    l => l.Label,
                    i => i.TypeName,
                    (label, intentType) => new SupportTask
                    {
                        Id = Guid.NewGuid(),
                        SupportConversationId = conversationId,
                        IntentTypeId = intentType.Id,
                        Status = SupportTaskStatus.New,
                        CreatedAt = DateTime.UtcNow,
                        TaskPiority = (int)(label.Confidence * 10)
                    })
                .ToList();

            if (tasks.Any())
                await taskRepo.InsertRangeAsync(tasks);
        });
        return true;
    }

    private async Task<bool> AssignStaffToConversationAsync(Guid conversationId)
    {
        var taskRepo = _unitOfWork.GetRepository<SupportTask>();
        var staffIntentRepo = _unitOfWork.GetRepository<StaffIntentType>();
        var conversationRepo = _unitOfWork.GetRepository<SupportConversation>();

        var tasks = await taskRepo.GetListAsync(
             predicate: t => t.SupportConversationId == conversationId,
             include: q => q.Include(t => t.IntentType)
         );

        if (!tasks.Any()) return false;

        var highestIntent = tasks
            .OrderByDescending(t => t.IntentType.IntentTypePiority)
            .First()
            .IntentTypeId;

        var candidates = await staffIntentRepo.GetListAsync(
            predicate: s =>
                s.IntentTypeId == highestIntent &&
                s.Staff.IsActive == true &&
                s.Staff.Status == StaffStatus.Online,
            include: q => q.Include(s => s.Staff)
        );

        if (!candidates.Any()) return false;

        var candidateIds = candidates.Select(x => x.StaffId).Distinct().ToList();

        var conversations = await conversationRepo.GetListAsync(
            predicate: c =>
                c.ActiveStaffId != null &&
                candidateIds.Contains(c.ActiveStaffId.Value) &&
                c.Status == ConversationStatus.Pending
        );

        var loadMap = conversations
            .GroupBy(c => c.ActiveStaffId)
            .ToDictionary(g => g.Key!.Value, g => g.Count());

        var staffLoads = candidates
            .Select(c => new
            {
                Staff = c.Staff,
                Load = loadMap.ContainsKey(c.StaffId) ? loadMap[c.StaffId] : 0
            })
            .ToList();

        var value = _config["SupportRouting:MaxPendingConversationPerStaff"];
        if (!int.TryParse(value, out int maxPending)) maxPending = 5;

        var availableStaff = staffLoads
            .Where(x => x.Load < maxPending)
            .ToList();

        if (!availableStaff.Any()) return false;

        var minLoad = availableStaff.Min(x => x.Load);
        var random = new Random();

        var selectedStaff = availableStaff
            .Where(x => x.Load == minLoad)
            .Select(x => x.Staff)
            .OrderBy(_ => random.Next())
            .First();

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            foreach (var task in tasks)
            {
                task.CurrentAssignedStaffId = selectedStaff.Id;
                task.Status = SupportTaskStatus.InProgress;
                task.IntentType = null;
            }

            taskRepo.UpdateRange(tasks);

            var conversation = await conversationRepo.SingleOrDefaultAsync(
                predicate: c => c.Id == conversationId && c.ActiveStaffId == null
            );

            if (conversation == null) return;

            if (conversation != null)
            {
                conversation.ActiveStaffId = selectedStaff.Id;
                conversation.IsDistributed = true;
                conversationRepo.Update(conversation);
            }
        });

        return true;
    }
    private async Task<PredictResponse?> AnalyzeAsync(PredictRequest predictRequest)
    {
        var apiUrl = _config["AIService:BaseUrl"];
        var apiKey = _config["AIService:ApiKey"];
        var apiName = _config["AIService:ApiName"];

        using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = JsonContent.Create(predictRequest)
        };

        request.Headers.Add(apiName ?? "omni-chat-api-key", apiKey);

        request.Headers.Add("ngrok-skip-browser-warning", "true");

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PredictResponse>();
    }
}
