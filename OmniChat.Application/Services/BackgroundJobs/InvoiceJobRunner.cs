using DocumentFormat.OpenXml.Spreadsheet;
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
        var now = DateTime.UtcNow;
        int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startOfWeek = now.Date.AddDays(-diff);

        var endOfWeek = startOfWeek.AddDays(6);

        var start = from ?? startOfWeek;
        var end = to ?? endOfWeek;

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
