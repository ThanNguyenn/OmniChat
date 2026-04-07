using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.BackgroundJobs
{
    public class ConversationWarningWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ConversationWarningWorker> _logger;
        private readonly IConfiguration _configuration;

        public ConversationWarningWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<ConversationWarningWorker> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndCreateWarningsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[WARNING-WORKER] Error during check");
                }

                // chạy mỗi 1 tiếng
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task CheckAndCreateWarningsAsync()
        {
            var minTaskSecs = int.Parse(_configuration["WarningThresholds:MinTaskDurationSeconds"] ?? "120");
            var minConvSecs = int.Parse(_configuration["WarningThresholds:MinConversationDurationSeconds"] ?? "300");
            var notRespondingMins = int.Parse(_configuration["WarningThresholds:StaffNotRespondingMinutes"] ?? "30");

            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<OmniChatDbContext>>();
            var warningsToAdd = new List<ConversationWarning>();

            // =============================================
            // CASE 1 & 2: Conversation/Task hoàn tất quá nhanh
            // =============================================
            var completedConversations = await unitOfWork.GetRepository<SupportConversation>()
                .GetListAsync(
                    predicate: c => c.Status == ConversationStatus.Complete
                                 && c.CloseAt != null
                                 && c.CreatedDate != null,
                    include: q => q
                        .Include(c => c.SupportTasks)
                        .Include(c => c.ConversationWarnings)
                );

            foreach (var conv in completedConversations)
            {
                if (conv.ActiveStaffId == null) continue;

                // Bỏ qua nếu đã có warning loại này rồi
                bool alreadyWarned = conv.ConversationWarnings.Any(w =>
                    w.WarningType == WarningType.TaskCompletedTooFast ||
                    w.WarningType == WarningType.ConversationClosedTooFast ||
                    w.WarningType == WarningType.BothFast);

                if (alreadyWarned) continue;

                var convDuration = (conv.CloseAt!.Value - conv.CreatedDate!.Value).TotalSeconds;
                bool convTooFast = convDuration < minConvSecs;

                var fastTasks = conv.SupportTasks
                    .Where(t => t.Status == SupportTaskStatus.Done
                             && t.CompleteDate != null
                             && t.CreatedAt != null
                             && (t.CompleteDate!.Value - t.CreatedAt!.Value).TotalSeconds < minTaskSecs)
                    .ToList();

                bool taskTooFast = fastTasks.Any();

                if (!convTooFast && !taskTooFast) continue;

                WarningType warningType;
                string reason;

                if (convTooFast && taskTooFast)
                {
                    warningType = WarningType.BothFast;
                    reason = $"Conversation hoàn tất sau {convDuration:F0}s (ngưỡng {minConvSecs}s). " +
                             $"Có {fastTasks.Count} task hoàn thành dưới {minTaskSecs}s.";
                }
                else if (convTooFast)
                {
                    warningType = WarningType.ConversationClosedTooFast;
                    reason = $"Conversation hoàn tất sau {convDuration:F0}s (ngưỡng {minConvSecs}s).";
                }
                else
                {
                    warningType = WarningType.TaskCompletedTooFast;
                    reason = $"Có {fastTasks.Count} task hoàn thành dưới {minTaskSecs}s.";
                }

                warningsToAdd.Add(new ConversationWarning
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv.Id,
                    StaffId = conv.ActiveStaffId.Value,
                    WarningType = warningType,
                    Reason = reason,
                    IsReviewed = false,
                    CreatedAt = DateTime.UtcNow
                });

                _logger.LogInformation(
                    "[WARNING-WORKER] Fast completion warning | Conv={ConvId} | Staff={StaffId} | Type={Type}",
                    conv.Id, conv.ActiveStaffId, warningType);
            }

            // =============================================
            // CASE 3: Staff không trả lời customer
            // =============================================
            var activeConversations = await unitOfWork.GetRepository<SupportConversation>()
                .GetListAsync(
                    predicate: c =>
                        (c.Status == ConversationStatus.Pending || c.Status == ConversationStatus.Waiting)
                        && c.ActiveStaffId != null
                        && c.LastCustomerMessageAt != null,
                    include: q => q.Include(c => c.ConversationWarnings)
                );

            var threshold = DateTime.UtcNow.AddMinutes(-notRespondingMins);

            foreach (var conv in activeConversations)
            {
                bool customerMessagedLongAgo = conv.LastCustomerMessageAt < threshold;
                bool staffNotRepliedAfter = conv.LastStaffMessageAt == null
                                            || conv.LastStaffMessageAt < conv.LastCustomerMessageAt;

                if (!customerMessagedLongAgo || !staffNotRepliedAfter) continue;

                // Chống spam: chỉ tạo 1 warning StaffNotResponding mỗi tiếng
                bool recentWarningExists = conv.ConversationWarnings.Any(w =>
                    w.WarningType == WarningType.StaffNotResponding &&
                    w.CreatedAt >= DateTime.UtcNow.AddHours(-1));

                if (recentWarningExists) continue;

                var waitingMinutes = (DateTime.UtcNow - conv.LastCustomerMessageAt!.Value).TotalMinutes;

                warningsToAdd.Add(new ConversationWarning
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv.Id,
                    StaffId = conv.ActiveStaffId!.Value,
                    WarningType = WarningType.StaffNotResponding,
                    Reason = $"Staff chưa trả lời customer sau {waitingMinutes:F0} phút " +
                                     $"(ngưỡng {notRespondingMins} phút). " +
                                     $"Tin nhắn cuối của customer lúc {conv.LastCustomerMessageAt:HH:mm dd/MM/yyyy}.",
                    IsReviewed = false,
                    CreatedAt = DateTime.UtcNow
                });

                _logger.LogWarning(
                    "[WARNING-WORKER] Staff not responding | Conv={ConvId} | Staff={StaffId} | WaitingMins={Mins:F0}",
                    conv.Id, conv.ActiveStaffId, waitingMinutes);
            }

            // =============================================
            // Lưu tất cả warnings
            // =============================================
            if (warningsToAdd.Any())
            {
                await unitOfWork.GetRepository<ConversationWarning>().InsertRangeAsync(warningsToAdd);
                await unitOfWork.CommitAsync();
                _logger.LogInformation("[WARNING-WORKER] Inserted {Count} warnings", warningsToAdd.Count);
            }
            else
            {
                _logger.LogInformation("[WARNING-WORKER] No new warnings found");
            }
        }
    }
}
