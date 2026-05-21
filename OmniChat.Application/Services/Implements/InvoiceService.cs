using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Mail;
using OmniChat.Infrastructure.Dtos.Responses.Invoice;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class InvoiceService : BaseService<InvoiceService>, IInvoiceService
{
    private readonly IMailService _mailService;

    public InvoiceService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<InvoiceService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IMailService mailService) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _mailService = mailService;
    }

    public async Task AllocateMoneyToInvoices(Guid customerId)
    {
        var allocationRepo = _unitOfWork.GetRepository<Allocation>();
        var walletRepo = _unitOfWork.GetRepository<Wallet>();
        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();

        _unitOfWork.Context.ChangeTracker.Clear();

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var wallet = await walletRepo.SingleOrDefaultAsync(
                predicate: w => w.CustomerId == customerId
            );

            if (wallet == null || wallet.Amount <= 0)
                return;

            var invoices = await invoiceRepo.GetListAsync(
                predicate: i =>
                    i.CustomerId == customerId &&
                    i.InvoiceStatus != InvoiceStatus.Completed && 
                    i.InvoiceStatus != InvoiceStatus.Refunded && 
                    i.InvoiceStatus != InvoiceStatus.PendingRefund &&
                    !(i.IsDeleted ?? false),
                include: i => i.Include(x => x.Allocations)
                               .Include(x => x.CustomerProfile),
                orderBy: q => q.OrderBy(i => i.StartedDate)
            );

            foreach (var invoice in invoices)
            {
                var paidAmount = invoice.Allocations.Sum(a => a.Amount);
                var remaining = invoice.Total - paidAmount;

                if (remaining <= 0)
                {
                    invoice.InvoiceStatus = InvoiceStatus.Completed;
                    var mailContent = new MailContent
                    {
                        To = invoice.CustomerProfile.Email,
                        Subject = "Thông báo thanh toán hóa đơn",
                        Body = $"Hóa đơn của bạn :{invoice.InvoiceCode} đã được thành toán bằng ví"
                    };
                    await _mailService.SendEmailAsync(mailContent);
                    continue;
                }

                if (wallet.Amount <= 0)
                    break;

                var allocationAmount = Math.Min(wallet.Amount, remaining);

                await allocationRepo.InsertAsync(new Allocation
                {
                    InvoiceId = invoice.Id,
                    WalletId = wallet.Id,
                    Amount = allocationAmount,
                });
                invoice.PaidAmount += allocationAmount;
                wallet.Amount -= allocationAmount;

                if (allocationAmount == remaining)
                {
                    invoice.InvoiceStatus = InvoiceStatus.Completed;
                    invoice.CompletedDate = DateTime.UtcNow;
                }
                else
                {
                    invoice.InvoiceStatus = InvoiceStatus.Pending;
                }

                invoiceRepo.Update(invoice);
            }
            walletRepo.Update(wallet);
            //invoiceRepo.UpdateRange(invoices);
        });
    }

    public async Task<List<Guid>> CreateInvoice(DateTime from, DateTime to)
    {
        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var walletRepo = _unitOfWork.GetRepository<Wallet>();

        var invoicesToInsert = new List<Invoice>();

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var orders = await orderRepo.GetListAsync(predicate: o =>
                o.OrderDate >= from &&
                o.OrderDate <= to &&
                o.DeliveryStatus == DeliveryStatus.Completed &&
                o.InvoiceId == null &&
                !(o.IsDeleted ?? false)
                , include: o => o.Include(x => x.CreditNotes.Where(cn =>
            cn.CreditNoteType == CreditNoteType.Adjustment))
            );
            _logger.LogInformation("Orders found: {count}", orders.Count);
            if (!orders.Any())
            {
                _logger.LogInformation("No orders found for the specified time range. No invoices will be created.");
                return;
            }
            var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();

            //var existingInvoices = await invoiceRepo.GetListAsync(predicate: i =>
            //    customerIds.Contains(i.CustomerId) &&
            //    i.StartedDate == from &&
            //    i.EndedDate == to &&
            //    !(i.IsDeleted ?? false)
            //);

            //var existingCustomerSet = existingInvoices
            //    .Select(i => i.CustomerId)
            //    .ToHashSet();

            //foreach (var invoice in existingInvoices)
            //{
            //    _logger.LogInformation("Existing invoice found for CustomerId: {customerId}, InvoiceId: {invoiceId}", invoice.CustomerId, invoice.Id);
            //}
            var grouped = orders.GroupBy(o => o.CustomerId);
            _logger.LogInformation("Processing {groupCount} customer groups for invoice creation.", grouped.Count());
            var usedCreditNotes = new List<CreditNote>();
            foreach (var group in grouped)
            {
                var customerId = group.Key;

                //if (existingCustomerSet.Contains(customerId))
                //    continue;

                var customerOrders = group.ToList();
                var orderTotal = customerOrders.Sum(o =>
                {
                    var adjustment = 0d;

                    var pendingNotes = o.CreditNotes?
                        .Where(cn => cn.CreditNoteStatus == CreditNoteStatus.Pending)
                        .ToList() ?? new List<CreditNote>();

                    foreach (var cn in pendingNotes)
                    {
                        adjustment += cn.Total;
                        usedCreditNotes.Add(cn);
                    }

                    return o.TotalAmount - adjustment;
                });
                var wallet = await walletRepo.SingleOrDefaultAsync(predicate:
                w => w.CustomerId == customerId);

                var walletBalance = wallet?.Amount ?? 0;

                var deduction = Math.Min(orderTotal, walletBalance);

                invoicesToInsert.Add(new Invoice
                {
                    CustomerId = customerId,
                    StartedDate = from,
                    EndedDate = to,
                    Total = orderTotal,
                    DeductedAmount = deduction,
                    InvoiceStatus = InvoiceStatus.Pending,
                    CreateAt = DateTime.UtcNow
                });

                _logger.LogInformation("Prepared invoice for CustomerId: {customerId}, Total: {total}, DeductedAmount: {deductedAmount}", customerId, orderTotal, deduction);
            }

            if (!invoicesToInsert.Any())
            {
                _logger.LogInformation("No new invoices to create after checking existing invoices.");
                return;
            }

            await invoiceRepo.InsertRangeAsync(invoicesToInsert);
            foreach (var cn in usedCreditNotes)
            {
                cn.CreditNoteStatus = CreditNoteStatus.Completed;
            }
            await _unitOfWork.CommitAsync();

            var invoiceByCustomer = invoicesToInsert
                .ToDictionary(i => i.CustomerId, i => i.Id);

            foreach (var order in orders)
            {
                if (invoiceByCustomer.TryGetValue(order.CustomerId, out var invoiceId))
                {
                    order.InvoiceId = invoiceId;
                }
            }

            orderRepo.UpdateRange(orders);
        });
        _unitOfWork.Context.ChangeTracker.Clear();
        return invoicesToInsert
            .Select(i => i.CustomerId)
            .Distinct()
            .ToList();
    }
    public async Task<IEnumerable<DashBoardInvoiceByYearResponse>> GetTotalIncomeAsync(string input)
    {
        bool isYear = input.Length == 4;

        int year;
        int? month = null;

        if (isYear)
        {
            year = int.Parse(input);
        }
        else
        {
            var parts = input.Split('/');
            month = int.Parse(parts[0]);
            year = int.Parse(parts[1]);
        }

        var monthsToProcess = isYear
            ? Enumerable.Range(1, 12)
            : new[] { month.Value };

        var result = new List<DashBoardInvoiceByYearResponse>();

        foreach (var m in monthsToProcess)
        {
            var from = new DateTime(year, m, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddMonths(1);

            var total = await TotalIncomeByTime(from, to);

            result.Add(new DashBoardInvoiceByYearResponse
            {
                Month = $"{m:D2}/{year}",
                TotalAmount = total
            });
        }

        return result;
    }
    private async Task<double> TotalIncomeByTime(DateTime from, DateTime to)
    {
        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();

        var fromDate = from.Date;
        var toDateExclusive = to.Date.AddDays(1);

        return await invoiceRepo.GetQueryable(
                i => i.CompletedDate.HasValue &&
                     i.CompletedDate.Value >= fromDate &&
                     i.CompletedDate.Value < toDateExclusive &&
                     (i.InvoiceStatus == InvoiceStatus.Completed ||
                      i.InvoiceStatus == InvoiceStatus.PartialPaid),
                asNoTracking: true
            )
            .SumAsync(i => (double?)i.PaidAmount) ?? 0;
    }

    public async Task<IEnumerable<DashBoardInvoiceByYearResponse>> GetTotalUnpaidAsync(string input)
    {
        bool isYear = input.Length == 4;

        int year;
        int? month = null;

        if (isYear)
        {
            year = int.Parse(input);
        }
        else
        {
            var parts = input.Split('/');
            month = int.Parse(parts[0]);
            year = int.Parse(parts[1]);
        }

        var monthsToProcess = isYear
            ? Enumerable.Range(1, 12)
            : new[] { month.Value };

        var result = new List<DashBoardInvoiceByYearResponse>();

        foreach (var m in monthsToProcess)
        {
            var from = new DateTime(year, m, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddMonths(1);

            var total = await TotalUnpaidAmountByTime(from, to);

            result.Add(new DashBoardInvoiceByYearResponse
            {
                Month = $"{m:D2}/{year}",
                TotalAmount = total
            });
        }

        return result;
    }


    private async Task<double> TotalUnpaidAmountByTime(DateTime from, DateTime to)
    {
        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();

        var fromDate = from.Date;
        var toDateExclusive = to.Date.AddDays(1);

        return await invoiceRepo.GetQueryable(
                i => i.CreateAt >= fromDate &&
                     i.CreateAt < toDateExclusive &&
                     (i.InvoiceStatus == InvoiceStatus.Pending ||
                      i.InvoiceStatus == InvoiceStatus.PartialPaid),
                asNoTracking: true
            )
            .SumAsync(i => (double?)(i.Total - i.PaidAmount - i.DeductedAmount)) ?? 0;
    }

    public async Task<PagingResponse<GetInvoicesResponse>> GetInvoicesAsync(Guid? customerId, string? customerName, InvoiceStatus? status, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false)
    {
        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();
        var response = await invoiceRepo.GetPagingListAsync<GetInvoicesResponse>(
                    predicate: p =>
    (customerId == null || p.CustomerId == customerId)
    && (status == null || p.InvoiceStatus == status)
    && (string.IsNullOrEmpty(customerName)
        || (p.CustomerProfile != null &&
            p.CustomerProfile.CustomerName.ToLower().Contains(customerName.ToLower()))),
                    orderBy: q => OrderBy(q, sortBy, descending),
                    selector: e => _mapper.Map<GetInvoicesResponse>(e),
                    page: pageNumber,

                    size: pageSize, include: i => i.Include(x => x.CustomerProfile)
                );
        return response;
    }

    private static IOrderedQueryable<Invoice> OrderBy(IQueryable<Invoice> query, string sortBy, bool descending)
    {
        sortBy = sortBy?.Trim().ToLower() ?? "createdate";

        return (sortBy, descending) switch
        {
            ("startdate", false) => query.OrderBy(s => s.StartedDate),
            ("startdate", true) => query.OrderByDescending(s => s.StartedDate),
            ("endeddate", false) => query.OrderBy(s => s.EndedDate),
            ("endeddate", true) => query.OrderByDescending(s => s.EndedDate),
            ("total", false) => query.OrderBy(s => s.Total),
            ("total", true) => query.OrderByDescending(s => s.Total),
            ("status", false) => query.OrderBy(s => s.InvoiceStatus),
            ("status", true) => query.OrderByDescending(s => s.InvoiceStatus),
            ("id", false) => query.OrderBy(s => s.Id),
            ("id", true) => query.OrderByDescending(s => s.Id),
            (_, false) => query.OrderBy(s => s.CreateAt),
            (_, true) => query.OrderByDescending(s => s.CreateAt)
        };
    }
    public async Task<GetInvoiceResponse> GetInvoiceAsync(Guid invoiceId)
    {
        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();
        var invoice = await invoiceRepo.SingleOrDefaultAsync(predicate: a => a.Id == invoiceId, include: i => i.Include(x => x.CustomerProfile)) ?? throw new NotFoundException("Không tìm thấy phiếu thanh toán");
        return _mapper.Map<GetInvoiceResponse>(invoice);
    }


}
