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
using System.Text.Json;

namespace OmniChat.Application.Services.Implements;

public class TaskAssignmentService : BaseService<TaskAssignmentService>, ITaskAssignmentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IKeywordService _keywordService;
    private static readonly SemaphoreSlim _assignmentLock = new SemaphoreSlim(1, 1);
    public TaskAssignmentService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<TaskAssignmentService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IConfiguration config, IKeywordService keywordService   ) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _config = config;
        _httpClient = httpClient;
        _keywordService = keywordService;
    }

    public async Task<bool> ProcessTask(PredictRequest predictRequest, Guid conversationId)
    {
        var (predict, hasIntent) = await AnalyzeAsync(predictRequest);

        // Return false if no intent was detected or the result is null
        if (!hasIntent || predict == null)
        {
            return false;
        }

        var isSuccessfullyCreatingTask = await CreateTaskAsync(predict, conversationId);
        if (!isSuccessfullyCreatingTask)
        {
            _logger.LogWarning("No tasks created for conversation {ConversationId} after intent analysis", conversationId);
            return true; // Return true because prediction technically succeeded, even if task creation failed
        }

        var assigned = await AssignStaffToConversationAsync(conversationId);

        if (!assigned)
        {
            _unitOfWork.Context.ChangeTracker.Clear();
            await ProcessWaitingQueueAsync();
        }

        return true; // Success case
    }

    public async Task ProcessWaitingQueueAsync()
    {
        if (!await _assignmentLock.WaitAsync(0))
            return;

        try
        {
            var conversationRepo = _unitOfWork.GetRepository<SupportConversation>();

            var conversations = await conversationRepo.GetListAsync(
                predicate: c =>
                    c.Status == ConversationStatus.Pending &&
                    !c.IsDistributed &&
                    c.SupportTasks.Any(),
                orderBy: q => q.OrderBy(c => c.CreatedDate)
            );
            _logger.LogInformation("Processing waiting queue. Found {Count} conversations.", conversations.Count);

            foreach (var conversation in conversations)
            {
                bool assigned = await AssignStaffToConversationAsync(conversation.Id);

                if (!assigned)
                {
                    _logger.LogWarning(
                        "No staff available for {ConversationId}",
                        conversation.Id
                    );

                    continue;
                }
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
        {  
            _logger.LogWarning("No predicted intents for conversation {ConversationId}", conversationId);
            return false;
        }
      
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
            _unitOfWork.Context.ChangeTracker.Clear();
            foreach (var task in tasks)
            {
                task.CurrentAssignedStaffId = selectedStaff.Id;
                task.Status = SupportTaskStatus.InProgress;
                task.IntentType = null;
                task.SupportConversation = null;
                taskRepo.Update(task);
            }

            var conversation = await conversationRepo.SingleOrDefaultAsync(
                predicate: c => c.Id == conversationId && c.ActiveStaffId == null
            );

            if (conversation == null) return;

            if (conversation != null)
            {
                conversation.ActiveStaffId = selectedStaff.Id;
                conversation.IsDistributed = true;
                conversation.Staff = null;
                conversation.SupportTasks = null;
                conversationRepo.Update(conversation);
            }
        });

        return true;
    }
    private async Task<(PredictResponse? Response, bool HasIntent)> AnalyzeAsync(PredictRequest predictRequest)
    {
        var apiUrl = _config["AIService:BaseUrl"];
        var apiKey = _config["AIService:ApiKey"];
        var apiName = _config["AIService:ApiName"];

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = JsonContent.Create(predictRequest)
            };

            request.Headers.Add(apiName ?? "omni-chat-api-key", apiKey);
            request.Headers.Add("ngrok-skip-browser-warning", "true");

            var response = await _httpClient.SendAsync(request);
            var rawResponse = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("Response Body: {Body}", rawResponse);

            response.EnsureSuccessStatusCode();

            var aiResult = JsonSerializer.Deserialize<PredictResponse>(rawResponse);

            if (aiResult?.Details != null && aiResult.Details.Any(d => d.Predicted))
            {
                return (aiResult, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI analysis failed. Falling back to keyword analysis.");
        }

        var fallback = await _keywordService.AnalyzeMessageWithKeywordsAsync(predictRequest.Message);
        bool hasIntent = fallback?.Details != null && fallback.Details.Any(d => d.Predicted);

        return (fallback, hasIntent);
    }
}

