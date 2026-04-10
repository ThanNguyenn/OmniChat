using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.TaskAction;
using OmniChat.Infrastructure.Dtos.Requests.TaskCancelReason;
using OmniChat.Infrastructure.Dtos.Responses.SupportTask;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class SupportTaskService : BaseService<SupportTaskService>, ISupportTaskService
    {
        private readonly IStaffPerformanceService _staffPerformanceService;
        private readonly ITaskActionService _taskActionService;
     
        public SupportTaskService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<SupportTaskService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IStaffPerformanceService staffPerformanceService, ITaskActionService taskActionService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _staffPerformanceService = staffPerformanceService;
            _taskActionService = taskActionService;   
        }

        public async Task<IEnumerable<SupportTask>> GetDoneSupportTaskByConversationIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportTask>();
            var supportTasks = await repo.GetListAsync(
                predicate: x => x.SupportConversationId == conversationId
                && x.Status == SupportTaskStatus.Done);

            if (!supportTasks.Any())
            {
                throw new NotFoundException("No SupportTask Found");
            }

            return supportTasks;
        }

        public async Task<IEnumerable<SupportTasksResponse>> GetSupportTaskOnConversationIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportTask>();

            var activeStatuses = new[]
            {
                SupportTaskStatus.Done,
                SupportTaskStatus.InProgress,
                SupportTaskStatus.PendingReassign
              };

            var supportTasks = await repo.GetListAsync(
                predicate: x => x.SupportConversationId == conversationId
                             && activeStatuses.Contains(x.Status),
                include: x => x.Include(x => x.IntentType)
            );


            if (!supportTasks.Any())
                return Enumerable.Empty<SupportTasksResponse>();

            return _mapper.Map<IEnumerable<SupportTasksResponse>>(supportTasks);
        }


        public async Task<IEnumerable<SupportTask>> GetSupportTaskByConversationIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportTask>();

            var supportTasks = await repo.GetListAsync(
                predicate: x => x.SupportConversationId == conversationId
                );

            if (!supportTasks.Any())
            {
                throw new NotFoundException("No SupportTask Found");
            }
            return supportTasks;
        }

        public async Task<bool> CompleteTaskAsync(Guid taskId)
        {
            var repo = _unitOfWork.GetRepository<SupportTask>();
            var existSupportTask = await repo.GetByIdAsync(taskId);

            if (existSupportTask == null)
                throw new NotFoundException("No SupportTask found");

            if (existSupportTask.Status == SupportTaskStatus.Done)
                throw new BadRequestException("Task already completed");

            var now = DateTime.UtcNow;


            var handleTime = existSupportTask.CreatedAt.HasValue
                ? (int)(now - existSupportTask.CreatedAt.Value).TotalSeconds
                : 0;

            existSupportTask.Status = SupportTaskStatus.Done;
            existSupportTask.CompleteDate = now;
            repo.Update(existSupportTask);
            await _unitOfWork.CommitAsync();



            if (existSupportTask.CurrentAssignedStaffId.HasValue)
            {
                await _staffPerformanceService.UpdatePerformanceOnTaskCompleteAsync(
                    existSupportTask.CurrentAssignedStaffId.Value,
                    handleTime
                );

                var newAction = new TaskActionRequest
                {
                    SupportTaskId = existSupportTask.Id,
                    Action = TaskActionType.Completed,
                    ActionById = existSupportTask.CurrentAssignedStaffId.Value,
                    ActionToId = null,
                    Reason = $"Task completed by {existSupportTask.CurrentAssignedStaffId.Value}"
                };
                await _taskActionService.CreateTaskActionAsync(newAction);
            }

            return true;
        }

        public async Task<bool> CancelSupportTaskAsync(Guid taskId, TaskCancelReasonRequest cancelReasonRequest)
        {
            var repo = _unitOfWork.GetRepository<SupportTask>();
            var existSupportTask = await repo.GetByIdAsync(taskId);

            if (existSupportTask == null)
                throw new NotFoundException("No SupportTask found");

            if (existSupportTask.Status == SupportTaskStatus.Done ||
            existSupportTask.Status == SupportTaskStatus.Cancelled ||
            existSupportTask.Status == SupportTaskStatus.closed)
                throw new BadRequestException("Task already finalized");

            var now = DateTime.UtcNow;
            var handleTime = existSupportTask.CreatedAt.HasValue
                ? (int)(now - existSupportTask.CreatedAt.Value).TotalSeconds
                : 0;

            
            existSupportTask.Status = SupportTaskStatus.Cancelled;
            existSupportTask.CompleteDate = now;
            repo.Update(existSupportTask);

          
            cancelReasonRequest.SupportTaskId = taskId;
            var cancelReasonRepo = _unitOfWork.GetRepository<TaskCancelReason>();
            var cancelReason = _mapper.Map<TaskCancelReason>(cancelReasonRequest);
            await cancelReasonRepo.InsertAsync(cancelReason);

            
            await _unitOfWork.CommitAsync();

          
            if (existSupportTask.CurrentAssignedStaffId.HasValue)
            {
                await _staffPerformanceService.UpdatePerformanceOnTaskCancelAsync(
                     existSupportTask.CurrentAssignedStaffId.Value,
                     handleTime
                 );

                var newAction = new TaskActionRequest
                {
                    SupportTaskId = existSupportTask.Id,
                    Action = TaskActionType.Cancelled,
                    ActionById = existSupportTask.CurrentAssignedStaffId.Value,
                    ActionToId = null,
                    Reason = $"Task cancelled by {existSupportTask.CurrentAssignedStaffId.Value}"
                };
                await _taskActionService.CreateTaskActionAsync(newAction);
            }

            return true;
        }

        public async Task<IEnumerable<DashboardMonthResponse>> GetTaskIntentDashboardResponsesAsync(string year)
        {
            var yearInt = int.Parse(year);

            var from = new DateTime(yearInt, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(yearInt, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            var supportTaskRepo = _unitOfWork.GetRepository<SupportTask>();

            var rawData = await supportTaskRepo.GetQueryable(
                    t => t.CreatedAt.HasValue &&
                         t.CreatedAt.Value >= from &&
                         t.CreatedAt.Value <= to,
                    asNoTracking: true
                )
                .GroupBy(t => new
                {
                    Month = t.CreatedAt.Value.Month,
                    Intent = t.IntentType.TypeName
                })
                .Select(g => new
                {
                    g.Key.Month,
                    g.Key.Intent,
                    Count = g.Count()
                })
                .ToListAsync();

            var intents = new List<string>
            {
                "POST_SALE_CHANGE",
                "ORDER_STATUS",
                "PAYMENT",
                "PRE_SALE",
                "ORDER_CREATION"
            };

            var lookup = rawData.ToDictionary(
                x => (x.Month, x.Intent),
                x => x.Count
            );

            var result = new List<DashboardMonthResponse>();

            for (int month = 1; month <= 12; month++)
            {
                var monthIntents = new List<TaskIntentDashboardResponse>();

                foreach (var intent in intents)
                {
                    lookup.TryGetValue((month, intent), out var count);

                    monthIntents.Add(new TaskIntentDashboardResponse
                    {
                        IntentName = intent,
                        TaskCount = count
                    });
                }

                result.Add(new DashboardMonthResponse
                {
                    Month = month,
                    Intents = monthIntents
                });
            }

            return result;
        }
    }
}
