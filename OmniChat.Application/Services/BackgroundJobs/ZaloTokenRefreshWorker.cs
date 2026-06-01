using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.BackgroundJobs;

public sealed class ZaloTokenRefreshWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ZaloTokenRefreshWorker> _logger;

    public ZaloTokenRefreshWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ZaloTokenRefreshWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var zaloService =
                    scope.ServiceProvider.GetRequiredService<IZaloOAuthService>();

                await zaloService.RefreshAccessTokenAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh Zalo access token");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}

