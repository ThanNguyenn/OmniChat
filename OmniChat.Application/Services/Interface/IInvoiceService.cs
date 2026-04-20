using OmniChat.Infrastructure.Dtos.Responses.Invoice;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
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

    Task<PagingResponse<GetInvoicesResponse>> GetInvoicesAsync(Guid? customerId, string? customerName, InvoiceStatus? status, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false);

    Task<GetInvoiceResponse> GetInvoiceAsync(Guid invoiceId);
}
