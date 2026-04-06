using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.BackgroundJobs
{
    public class StaffPerformanceWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<StaffPerformanceWorker> _logger;

        public StaffPerformanceWorker(IServiceScopeFactory serviceScopeFactory,
                                      ILogger<StaffPerformanceWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = GetDelayUntilNextMonth();
                _logger.LogInformation("StaffPerformanceWorker sleeping for {Hours}h until next run at {Time}",
                    delay.TotalHours, DateTime.UtcNow.Add(delay));

                await Task.Delay(delay, stoppingToken);

                try
                {
                    _logger.LogInformation("StaffPerformanceWorker started at {Time}", DateTime.UtcNow);

                    using var scope = _serviceScopeFactory.CreateScope();
                    var performanceService = scope.ServiceProvider
                        .GetRequiredService<IStaffPerformanceService>();

                    await performanceService.InitializePerformanceForAllStaffAsync();

                    _logger.LogInformation("StaffPerformanceWorker completed at {Time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "StaffPerformanceWorker failed at {Time}", DateTime.UtcNow);
                }
            }
        }

        private TimeSpan GetDelayUntilNextMonth()
        {
            var now = DateTime.UtcNow;
            var nextMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMonths(1);
            return nextMonth - now;
        }
    }
}
