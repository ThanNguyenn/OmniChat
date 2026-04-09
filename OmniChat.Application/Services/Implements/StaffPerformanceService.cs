using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Performance;
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
    public class StaffPerformanceService : BaseService<StaffPerformanceService>, IStaffPerformanceService
    {


        public StaffPerformanceService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<StaffPerformanceService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
          
        }

        public async Task InitializePerformanceForStaffAsync(Guid staffId)
        {
            var now = DateTime.UtcNow;
            var fromTime = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var toTime = fromTime.AddMonths(1).AddTicks(-1);

            var repo = _unitOfWork.GetRepository<StaffPerformance>();


            var existing = await repo.SingleOrDefaultAsync(
                predicate: x => x.StaffId == staffId &&
                                x.FromTime == fromTime &&
                                x.ToTime == toTime
            );

            if (existing != null)
            {
                _logger.LogInformation("Performance already exists for staff {StaffId} in {Month}/{Year}",
                    staffId, now.Month, now.Year);
                return;
            }

            var performance = new StaffPerformance
            {
                Id = Guid.NewGuid(),
                StaffId = staffId,
                FromTime = fromTime,
                ToTime = toTime,
                CreateDate = now,
                UpdateDate = now
            };

            await repo.InsertAsync(performance);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Initialized performance for staff {StaffId} | {FromTime} - {ToTime}",
                staffId, fromTime, toTime);
        }

        public async Task InitializePerformanceForAllStaffAsync()
        {
            var now = DateTime.UtcNow;
            var fromTime = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var toTime = fromTime.AddMonths(1).AddTicks(-1);

            var staffRepo = _unitOfWork.GetRepository<Staff>();
            var performanceRepo = _unitOfWork.GetRepository<StaffPerformance>();

            var staffList = await staffRepo.GetListAsync(
                predicate: x => x.IsActive == true
            );


            var existingStaffIds = (await performanceRepo.GetListAsync(
                predicate: x => x.FromTime == fromTime && x.ToTime == toTime
            )).Select(x => x.StaffId).ToHashSet();

            var newPerformances = staffList
                .Where(s => !existingStaffIds.Contains(s.Id))
                .Select(s => new StaffPerformance
                {
                    Id = Guid.NewGuid(),
                    StaffId = s.Id,
                    FromTime = fromTime,
                    ToTime = toTime,
                    CreateDate = now,
                    UpdateDate = now
                }).ToList();

            if (!newPerformances.Any())
            {
                _logger.LogInformation("All staff already have performance for {Month}/{Year}",
                    now.Month, now.Year);
                return;
            }

            await performanceRepo.InsertRangeAsync(newPerformances);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Initialized performance for all staff | Total: {Count}",
                newPerformances.Count);
        }

        public async Task UpdatePerformanceOnTaskCompleteAsync(Guid staffId, int handleTimeSeconds)
        {
            var now = DateTime.UtcNow;
            var repo = _unitOfWork.GetRepository<StaffPerformance>();

            var performance = await repo.SingleOrDefaultAsync(
                predicate: x => x.StaffId == staffId &&
                                x.FromTime <= now &&
                                x.ToTime >= now
            );

            if (performance == null)
            {
                _logger.LogWarning("No active performance found for staff {StaffId}", staffId);
                return;
            }

            performance.TaskCompleted += 1;
            performance.TotalTaskHandleTime += handleTimeSeconds;
            performance.AvgTaskHandleTime = (double)performance.TotalTaskHandleTime / performance.TaskCompleted;
            performance.UpdateDate = now;

            repo.Update(performance);
            await _unitOfWork.CommitAsync();
        }

        public async Task UpdatePerformanceOnTaskCancelAsync(Guid staffId, int handleTimeSeconds)
        {
            var now = DateTime.UtcNow;
            var repo = _unitOfWork.GetRepository<StaffPerformance>();
            var performance = await repo.SingleOrDefaultAsync(
                predicate: x => x.StaffId == staffId &&
                                x.FromTime <= now &&
                                x.ToTime >= now
            );
            if (performance == null)
            {
                _logger.LogWarning("No active performance found for staff {StaffId}", staffId);
                return;
            }
            performance.CancelledCount += 1;
            performance.UpdateDate = now;
            repo.Update(performance);
            await _unitOfWork.CommitAsync();
        }

        public async Task UpdatePerformanceOnConversationCompleteAsync(Guid staffId, int firstResponseTimeSeconds)
        {
            var now = DateTime.UtcNow;
            var repo = _unitOfWork.GetRepository<StaffPerformance>();

            var performance = await repo.SingleOrDefaultAsync(
                predicate: x => x.StaffId == staffId &&
                                x.FromTime <= now &&
                                x.ToTime >= now
            );

            if (performance == null)
            {
                _logger.LogWarning("No active performance found for staff {StaffId}", staffId);
                return;
            }

            performance.ConversationOwned += 1;
            performance.TotalFirstResponseTime += firstResponseTimeSeconds;
            performance.AvgFirstResponseTime = (double)performance.TotalFirstResponseTime / performance.ConversationOwned;
            performance.UpdateDate = now;

            repo.Update(performance);
            await _unitOfWork.CommitAsync();
        }

        public async Task CompleteConversationAndTasksAsync(SupportConversation conversation)
        {
            var now = DateTime.UtcNow;
            var taskRepo = _unitOfWork.GetRepository<SupportTask>();

            var tasks = await taskRepo.GetListAsync(
                predicate: t => t.SupportConversationId == conversation.Id &&
                                t.Status != SupportTaskStatus.Done &&
                                t.Status != SupportTaskStatus.Cancelled
            );

            foreach (var task in tasks)
            {
                var handleTime = task.CreatedAt.HasValue
                    ? (int)(now - task.CreatedAt.Value).TotalSeconds
                    : 0;

                task.Status = SupportTaskStatus.Done;
                task.CompleteDate = now;
                taskRepo.Update(task);

                if (task.CurrentAssignedStaffId.HasValue)
                {
                    await UpdatePerformanceOnTaskCompleteAsync(task.CurrentAssignedStaffId.Value, handleTime);
                }

                _logger.LogInformation("[PERFORMANCE] Completed TaskId={TaskId} for StaffId={StaffId}",
                    task.Id, task.CurrentAssignedStaffId);
            }

           
            if (conversation.ActiveStaffId.HasValue &&
                conversation.FirstResponseAt.HasValue &&
                conversation.CreatedDate.HasValue)
            {
                var firstResponseTime = (int)(conversation.FirstResponseAt.Value - conversation.CreatedDate.Value).TotalSeconds;
                await UpdatePerformanceOnConversationCompleteAsync(conversation.ActiveStaffId.Value, firstResponseTime);
            }

            await _unitOfWork.CommitAsync();
        }

        public  async Task<TotalAverageResponse> GetTotalAverageAsync(DateTime fromDate, DateTime toDate)
        {
            var perRepo = _unitOfWork.GetRepository<StaffPerformance>();
            var supportTaskRepo = _unitOfWork.GetRepository<SupportTask>();
            var staffMessageRepo = _unitOfWork.GetRepository<SupportStaffMessage>();
            var supportConversationRepo = _unitOfWork.GetRepository<SupportConversation>();
            var customerMessageRepo = _unitOfWork.GetRepository<CustomerMessage>();

            var fromTimestamp = new DateTimeOffset(fromDate).ToUnixTimeMilliseconds();
            var toTimestamp = new DateTimeOffset(toDate).ToUnixTimeMilliseconds();

            var totalCustomerMessages = await customerMessageRepo.CountAsync(
                predicate: m => m.Timestamp >= fromTimestamp && m.Timestamp <= toTimestamp
            );

            var conversations = await supportConversationRepo.GetListAsync(
                predicate: c => c.CreatedDate.HasValue &&
                                c.CreatedDate.Value >= fromDate &&
                                c.CreatedDate.Value <= toDate &&
                                c.FirstResponseAt != null,
                selector: c => new
                {
                    c.CreatedDate,
                    c.FirstResponseAt,
                    c.CloseAt
                }
            );

            double averageTotalResponseTime  = 0;

            if (conversations.Any())
            {                
                averageTotalResponseTime = conversations
                    .Where(c => c.FirstResponseAt.HasValue && c.CreatedDate.HasValue)
                    .Select(c => (c.FirstResponseAt!.Value - c.CreatedDate!.Value).TotalSeconds)
                    .DefaultIfEmpty(0)
                    .Average();
            }

            var completedTasks = await supportTaskRepo.GetListAsync(predicate:
               t => t.Status == SupportTaskStatus.Done &&
                    t.CompleteDate >= fromDate &&
                    t.CompleteDate <= toDate &&
                    t.CreatedAt != null &&
                    t.CompleteDate != null,
                    selector: t => new
                    {
                        t.CreatedAt,
                        t.CompleteDate
                    }
            );

            double totalAverageTaskComplete = 0;
            if (completedTasks.Any())
            {
                totalAverageTaskComplete = completedTasks
                    .Select(t => (t.CompleteDate!.Value - t.CreatedAt!.Value).TotalSeconds)
                    .DefaultIfEmpty(0)
                    .Average();
            }

            var closedConversations = await supportConversationRepo.GetListAsync(
                predicate: c => c.Status == ConversationStatus.Complete &&
                                c.CloseAt >= fromDate &&
                                c.CloseAt <= toDate &&
                                c.CreatedDate != null &&
                                c.CloseAt != null,
                selector: c => new
                {
                    c.CreatedDate,
                    c.CloseAt
                }
            );

            double totalAverageCompleteConversation = 0;
            if (closedConversations.Any())
            {
                totalAverageCompleteConversation = closedConversations
                    .Select(c => (c.CloseAt!.Value - c.CreatedDate!.Value).TotalSeconds)
                    .DefaultIfEmpty(0)
                    .Average();
            }

            return new TotalAverageResponse
            {
                AverageTotalResponseTime = Math.Round(averageTotalResponseTime, 2),
                TotalCustomerMessages = totalCustomerMessages,
                TotalAverageTaskComplete = Math.Round(totalAverageTaskComplete, 2),
                TotalAverageCompleteConversation = Math.Round(totalAverageCompleteConversation, 2)
            };
        }
    }
}
