using OmniChat.Infrastructure.Dtos.Responses.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IInvoiceService
{
    Task<List<Guid>> CreateInvoice(DateTime from, DateTime to);

    Task AllocateMoneyToInvoices(Guid invoiceId);


    Task<IEnumerable<DashBoardInvoiceByYearResponse>> GetTotalIncomeAsync(string input);
    Task<IEnumerable<DashBoardInvoiceByYearResponse>> GetTotalUnpaidAsync(string input);
}
