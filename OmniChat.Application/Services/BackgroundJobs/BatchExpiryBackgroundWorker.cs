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
        var interval = TimeSpan.FromHours(1);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();

            var productService = scope.ServiceProvider
                .GetRequiredService<IProductService>();

            await productService.UpdateBatchExpiryAsync();

            await Task.Delay(interval, stoppingToken);
        }
    }
}