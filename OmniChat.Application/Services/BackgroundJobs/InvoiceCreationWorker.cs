using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.BackgroundJobs;

public class InvoiceCreationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InvoiceCreationWorker> _logger;

    public InvoiceCreationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<InvoiceCreationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextRun = GetNextSundayNight();
            var delay = nextRun - DateTime.UtcNow;

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var runner = scope.ServiceProvider.GetRequiredService<InvoiceJobRunner>();

                await runner.RunAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invoice job failed");

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private DateTime GetNextSundayNight()
    {
        var now = DateTime.UtcNow;

        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;
        var nextSunday = now.Date.AddDays(daysUntilSunday).AddHours(23);

        if (nextSunday <= now)
            nextSunday = nextSunday.AddDays(7);

        return nextSunday;
    }

    private (DateTime from, DateTime to) GetInvoiceRange()
    {
        var to = DateTime.UtcNow.Date;
        var from = to.AddDays(-7);

        return (from, to);
    }
}
