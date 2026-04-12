using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.BackgroundJobs
{
    public class NotificationCleanupWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationCleanupWorker> _logger;

        public NotificationCleanupWorker(IServiceScopeFactory scopeFactory, ILogger<NotificationCleanupWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Cleanup Service đã khởi tạo.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Bắt đầu tiến trình dọn dẹp thông báo cũ: {time}", DateTimeOffset.Now);

                    // Tạo scope để có thể lấy ra Scoped Service (INotificationService)
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        // Gọi hàm xóa trong Service của bạn
                        await notificationService.DeleteNofiticationIsReadAsync();
                    }

                    _logger.LogInformation("Dọn dẹp thông báo hoàn tất. Đợi chu kỳ tiếp theo.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong NotificationCleanupService.");
                }

                // Đợi 30 ngày (Bạn có thể dùng TimeSpan.FromMinutes(1) để test thử trước)
                await Task.Delay(TimeSpan.FromDays(30), stoppingToken);
            }
        }
    }
}
