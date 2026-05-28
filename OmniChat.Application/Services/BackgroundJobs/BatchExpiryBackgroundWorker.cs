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
        await RunJobAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                var nextRun = now.Date.AddDays(1).AddMinutes(1);

                var delay = nextRun - now;

                await Task.Delay(delay, stoppingToken);

                await RunJobAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
            }
        }
    }

    private async Task RunJobAsync()
    {
        using var scope = _serviceProvider.CreateScope();

        var productService = scope.ServiceProvider
            .GetRequiredService<IProductService>();

        await productService.UpdateBatchExpiryAsync();
    }
}