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
            _logger.LogInformation("[WARNING-WORKER] Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndCreateWarningsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[WARNING-WORKER] Error during check cycle");
                }

                // Chạy kiểm tra định kỳ mỗi 1 tiếng
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task CheckAndCreateWarningsAsync()
        {
            // 1. CHỈ CHẠY TRONG GIỜ HÀNH CHÍNH
            if (!IsWorkingHours())
            {
                _logger.LogInformation("[WARNING-WORKER] Ngoài giờ làm việc (VN Time). Tạm dừng quét.");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<OmniChatDbContext>>();

            var notRespondingMins = int.Parse(_configuration["WarningThresholds:StaffNotRespondingMinutes"] ?? "30");
            var maxAvgTaskTime = int.Parse(_configuration["WarningThresholds:MaxAvgTaskHandleTimeSeconds"] ?? "600");

            var warningsToAdd = new List<ConversationWarning>();
            var nowUtc = DateTime.UtcNow;

            // 2. LẤY DỮ LIỆU CẦN THIẾT
            // Chỉ lấy các cuộc hội thoại ĐANG HOẠT ĐỘNG
            var activeConversations = await unitOfWork.GetRepository<SupportConversation>()
                .GetListAsync(
                    predicate: c =>
                        (c.Status == ConversationStatus.Pending ||
                        c.Status == ConversationStatus.Warning)
                        && c.ActiveStaffId != null,
                    include: q => q.Include(c => c.ConversationWarnings)
                );

            if (!activeConversations.Any()) return;

            // Lấy danh sách StaffId đang trực để lấy Performance một lần duy nhất (Tối ưu performance DB)
            var activeStaffIds = activeConversations.Select(c => c.ActiveStaffId!.Value).Distinct().ToList();
            var staffPerformances = await unitOfWork.GetRepository<StaffPerformance>()
                .GetListAsync(predicate: p => activeStaffIds.Contains(p.StaffId));

            foreach (var conv in activeConversations)
            {
                // --- TRƯỜNG HỢP 1: STAFF KHÔNG TRẢ LỜI QUÁ THỜI GIAN QUY ĐỊNH ---
                if (conv.LastCustomerMessageAt != null)
                {
                    var threshold = nowUtc.AddMinutes(-notRespondingMins);

                    // Staff im lặng: Chưa từng trả lời HOẶC tin nhắn cuối của staff cũ hơn tin nhắn của khách
                    bool staffIsSilent = conv.LastStaffMessageAt == null || conv.LastStaffMessageAt < conv.LastCustomerMessageAt;

                    if (conv.LastCustomerMessageAt < threshold && staffIsSilent)
                    {
                        // Chống spam: Mỗi 1 tiếng mới tạo thêm 1 warning StaffNotResponding cho cùng 1 hội thoại
                        bool recentlyWarned = conv.ConversationWarnings.Any(w =>
                            w.WarningType == WarningType.StaffNotResponding &&
                            w.CreatedAt >= nowUtc.AddHours(-1));

                        if (!recentlyWarned)
                        {
                            var waitingMins = (nowUtc - conv.LastCustomerMessageAt.Value).TotalMinutes;
                            warningsToAdd.Add(new ConversationWarning
                            {
                                Id = Guid.NewGuid(),
                                ConversationId = conv.Id,
                                StaffId = conv.ActiveStaffId!.Value,
                                WarningType = WarningType.StaffNotResponding,
                                Reason = $"Staff chưa phản hồi sau {waitingMins:F0} phút (Ngưỡng {notRespondingMins}m).",
                                IsReviewed = false,
                                CreatedAt = nowUtc
                            });
                        }
                    }
                }

                // --- TRƯỜNG HỢP 2: WARNING DỰA TRÊN PERFORMANCE (Xử lý quá chậm) ---
                var perf = staffPerformances.FirstOrDefault(p => p.StaffId == conv.ActiveStaffId);

                if (perf != null && perf.AvgTaskHandleTime > maxAvgTaskTime)
                {
                    // Chống spam: Chỉ tạo warning hiệu suất 1 lần mỗi ngày cho 1 cuộc hội thoại
                    bool performanceWarnedToday = conv.ConversationWarnings.Any(w =>
                        w.WarningType == WarningType.SlowPerformance &&
                        w.CreatedAt >= nowUtc.Date);

                    if (!performanceWarnedToday)
                    {
                        warningsToAdd.Add(new ConversationWarning
                        {
                            Id = Guid.NewGuid(),
                            ConversationId = conv.Id,
                            StaffId = conv.ActiveStaffId!.Value,
                            WarningType = WarningType.SlowPerformance,
                            Reason = $"Staff xử lý chậm: AvgTaskHandleTime là {perf.AvgTaskHandleTime:F0}s (Ngưỡng {maxAvgTaskTime}s).",
                            IsReviewed = false,
                            CreatedAt = nowUtc
                        });
                    }
                }
            }

            // 3. LƯU VÀO DATABASE
            if (warningsToAdd.Any())
            {
                await unitOfWork.GetRepository<ConversationWarning>().InsertRangeAsync(warningsToAdd);
                await unitOfWork.CommitAsync();
                _logger.LogInformation("[WARNING-WORKER] Successfully inserted {Count} new warnings.", warningsToAdd.Count);
            }
        }

        private bool IsWorkingHours()
        {
            // Chuyển sang múi giờ VN (UTC+7) để logic luôn đúng dù server đặt ở đâu
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            var time = vnNow.TimeOfDay;

            // Nghỉ tối & sáng sớm: 17:00 chiều -> 07:00 sáng hôm sau
            if (time >= new TimeSpan(17, 0, 0) || time < new TimeSpan(7, 0, 0)) return false;

            // Nghỉ trưa: 11:45 -> 12:00
            if (time >= new TimeSpan(11, 45, 0) && time < new TimeSpan(12, 0, 0)) return false;

            return true;
        }
    }
}

