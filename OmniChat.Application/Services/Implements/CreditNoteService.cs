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
    public CreditNoteService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<CreditNoteService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task<bool> CreateCreditNoteAdjustmentAsync(Guid orderId, double amount)
    {
        if (amount <= 0)
            throw new BusinessException("Amount must be > 0");

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
            throw new BusinessException("Amount must be > 0");

        var creditNoteRepo = _unitOfWork.GetRepository<CreditNote>();
        var orderRepo = _unitOfWork.GetRepository<Order>();


        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var existingOrder = await orderRepo
                .GetQueryable(
                    predicate: o => o.Id == orderId,
                    include: q => q.Include(c => c.CustomerProfile)
                                   .ThenInclude(w => w.Wallet!)
                )
                .FirstOrDefaultAsync();
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
            return true;
        });
    }
}
