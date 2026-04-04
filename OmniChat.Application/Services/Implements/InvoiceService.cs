using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
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
    public InvoiceService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<InvoiceService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task AllocateMoneyToInvoices(Guid customerId)
    {
        var allocationRepo = _unitOfWork.GetRepository<Allocation>();
        var walletRepo = _unitOfWork.GetRepository<Wallet>();
        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();

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
                    !(i.IsDeleted ?? false),
                include: i => i.Include(x => x.Allocations),
                orderBy: q => q.OrderBy(i => i.StartedDate)
            );

            foreach (var invoice in invoices)
            {
                var paidAmount = invoice.Allocations.Sum(a => a.Amount);
                var remaining = invoice.Total - paidAmount;

                if (remaining <= 0)
                {
                    invoice.InvoiceStatus = InvoiceStatus.Completed;
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
            }

            walletRepo.Update(wallet);
            invoiceRepo.UpdateRange(invoices);
        });
    }

    public async Task<List<Guid>> CreateInvoice(DateTime from, DateTime to)
    {
        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();
        var orderRepo = _unitOfWork.GetRepository<Order>();

        var invoicesToInsert = new List<Invoice>();

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var orders = await orderRepo.GetListAsync(predicate: o =>
                o.OrderDate >= from &&
                o.OrderDate < to &&
                o.DeliveryStatus == DeliveryStatus.Completed &&
                o.InvoiceId == null &&
                !(o.IsDeleted ?? false)
            );

            if (!orders.Any())
                return;

            var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();

            var existingInvoices = await invoiceRepo.GetListAsync(predicate: i =>
                customerIds.Contains(i.CustomerId) &&
                i.StartedDate == from &&
                i.EndedDate == to &&
                !(i.IsDeleted ?? false)
            );

            var existingCustomerSet = existingInvoices
                .Select(i => i.CustomerId)
                .ToHashSet();

            var grouped = orders.GroupBy(o => o.CustomerId);

            foreach (var group in grouped)
            {
                var customerId = group.Key;

                if (existingCustomerSet.Contains(customerId))
                    continue;

                var customerOrders = group.ToList();
                var orderTotal = customerOrders.Sum(o => o.TotalAmount);

                invoicesToInsert.Add(new Invoice
                {
                    CustomerId = customerId,
                    StartedDate = from,
                    EndedDate = to,
                    Total = orderTotal,
                    InvoiceStatus = InvoiceStatus.Pending,
                    CreateAt = DateTime.UtcNow
                });
            }

            if (!invoicesToInsert.Any())
                return;

            await invoiceRepo.InsertRangeAsync(invoicesToInsert);

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

        return invoicesToInsert
            .Select(i => i.CustomerId)
            .Distinct()
            .ToList();
    }

}
