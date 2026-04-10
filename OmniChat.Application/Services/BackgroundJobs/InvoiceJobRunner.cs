using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.BackgroundJobs;

public class InvoiceJobRunner
{
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<InvoiceJobRunner> _logger;

    public InvoiceJobRunner(
        IInvoiceService invoiceService,
        ILogger<InvoiceJobRunner> logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    public async Task RunAsync(DateTime? from = null, DateTime? to = null)
    {
        var end = to ?? DateTime.UtcNow.Date;
        var start = from ?? end.AddDays(-7);

        List<Guid> customerIds;

        try
        {
            customerIds = await _invoiceService.CreateInvoice(start, end);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invoice creation failed");
            throw;
        }

        foreach (var customerId in customerIds)
        {
            await _invoiceService.AllocateMoneyToInvoices(customerId);
        }
    }
}
