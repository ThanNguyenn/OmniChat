using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OmniChat.Application.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.BackgroundJobs;

public class BatchExpiryBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public BatchExpiryBackgroundWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            var nextRun = now.Date.AddDays(1).AddMinutes(1);

            var delay = nextRun - now;

            if (delay < TimeSpan.Zero)
                delay = TimeSpan.Zero;

            await Task.Delay(delay, stoppingToken);

            using var scope = _serviceProvider.CreateScope();

            var productService = scope.ServiceProvider
                .GetRequiredService<IProductService>();

            await productService.UpdateBatchExpiryAsync();
        }
    }
}