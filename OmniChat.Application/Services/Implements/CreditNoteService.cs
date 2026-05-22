using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class CreditNoteService : BaseService<CreditNoteService>, ICreditNoteService
{
    private readonly IInvoiceService _invoiceService;
    public CreditNoteService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<CreditNoteService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IInvoiceService invoiceService) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _invoiceService = invoiceService;
    }

    public async Task<bool> CreateCreditNoteAdjustmentAsync(Guid orderId, double amount)
    {
        if (amount <= 0)
            throw new BusinessException("Số lượng phải > 0");

        var creditNoteRepo = _unitOfWork.GetRepository<CreditNote>();
        var creditNote = new CreditNote
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Total = amount,
            CreditNoteType = CreditNoteType.Adjustment,
            CreditNoteStatus = CreditNoteStatus.Pending,
        };
        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            await creditNoteRepo.InsertAsync(creditNote);
            return true;
        });
    }

    public async Task<bool> CreateCreditNoteRefundAsync(Guid orderId, double amount)
    {
        if (amount <= 0)
            throw new BusinessException("Số lượng phải > 0");

        var creditNoteRepo = _unitOfWork.GetRepository<CreditNote>();
        var orderRepo = _unitOfWork.GetRepository<Order>();

        var existingOrder = await orderRepo
               .GetQueryable(
                   predicate: o => o.Id == orderId,
                   include: q => q.Include(c => c.CustomerProfile)
                                  .ThenInclude(w => w.Wallet!).Include(c => c.Invoice)
               )
               .FirstOrDefaultAsync();
        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
           
            var creditNote = new CreditNote
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Total = amount,
                CreditNoteType = CreditNoteType.Refund,
                CreditNoteStatus = CreditNoteStatus.Completed,
            };
            await creditNoteRepo.InsertAsync(creditNote);

            var wallet = existingOrder!.CustomerProfile!.Wallet!;
            wallet.Transactions.Add(new Transaction
            {
                WalletId = wallet.Id,
                Amount = amount,
                TransactionType = TransactionType.Credit
            });
            wallet.Amount += amount;
            existingOrder.Invoice.InvoiceStatus = InvoiceStatus.Refunded;

            orderRepo.Update(existingOrder);
            await _unitOfWork.CommitAsync();
        });

        await _invoiceService.AllocateMoneyToInvoices(existingOrder.CustomerId);

        return true;
    }
}
