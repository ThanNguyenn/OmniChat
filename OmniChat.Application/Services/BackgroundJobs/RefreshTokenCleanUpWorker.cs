using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace OmniChat.Application.Services.BackgroundJobs;

public class RefreshTokenCleanUpWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ZaloTokenRefreshWorker> _logger;

    public RefreshTokenCleanUpWorker(
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
                var refreshTokenService =
                    scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();
                await refreshTokenService.DeleteExpiredRefreshTokensAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up refresh tokens");
            }
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
