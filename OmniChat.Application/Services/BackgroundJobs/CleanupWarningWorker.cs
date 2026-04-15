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
    public class CleanupWarningWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CleanupWarningWorker> _logger;

        public CleanupWarningWorker(IServiceProvider serviceProvider, ILogger<CleanupWarningWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleanup Warning Worker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                 
                    if (DateTime.Now.Day == 1)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var warningService = scope.ServiceProvider.GetRequiredService<IConversationWarningService>();
                            _logger.LogInformation("Starting monthly cleanup at: {time}", DateTimeOffset.Now);

                            await warningService.DeleteWarningAsync();

                            _logger.LogInformation("Monthly cleanup completed successfully.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up warnings.");
                }
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
