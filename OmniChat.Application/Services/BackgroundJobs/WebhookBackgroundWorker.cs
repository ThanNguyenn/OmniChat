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
    public class WebhookBackgroundWorker : BackgroundService
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly ILogger<WebhookBackgroundWorker> _logger;

        public WebhookBackgroundWorker(
            IBackgroundTaskQueue queue,
            ILogger<WebhookBackgroundWorker> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);

                _ = Task.Run(async () =>
                {
                    int retry = 0;
                    const int maxRetry = 3;

                    while (true)
                    {
                        try
                        {
                            await workItem(stoppingToken);
                            break; // success → thoát
                        }
                        catch (Exception ex)
                        {
                            retry++;

                            _logger.LogError(ex,
                                "Background job failed (retry {Retry}/{Max})",
                                retry, maxRetry);

                            if (retry >= maxRetry)
                            {
                                _logger.LogError("Job failed permanently ❌");
                                break;
                            }

                            // exponential backoff
                            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retry)));
                        }
                    }
                }, stoppingToken);
            }
        }
    }
}
