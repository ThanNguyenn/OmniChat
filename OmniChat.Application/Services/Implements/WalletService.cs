using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Wallet;
using OmniChat.Infrastructure.Dtos.Responses.Wallet;
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

public class WalletService : BaseService<WalletService>, IWalletService
{
    private IInvoiceService _invoiceService;
    public WalletService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<WalletService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IInvoiceService invoiceService) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _invoiceService = invoiceService;
    }

    public async Task<bool> CreateWallet(Guid customerId)
    {
        var walletRepo = _unitOfWork.GetRepository<Wallet>();
        var existingWallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId);
        if (existingWallet != null)
        {
            throw new BusinessException($"Ví đã tồn tại");
        }

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            await walletRepo.InsertAsync(new Wallet
            {
                CustomerId = customerId,
            });
        });
        return true;
    }

    public async Task<bool> DeleteWallet(Guid customerId)
    {
        var walletRepo = _unitOfWork.GetRepository<Wallet>();
        var wallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId) ?? throw new NotFoundException($"Ví đã tồn tại");

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            walletRepo.Delete(wallet);
        });
        return true;
    }

    public async Task<bool> DepositToWallet(WalletPaymentRequest walletPaymentRequest)
    {
        var walletRepo = _unitOfWork.GetRepository<Wallet>();
        var customerId = walletPaymentRequest.CustomerId;
        var amount = walletPaymentRequest.Amount;
        var wallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId) ?? throw new NotFoundException($"Ví đã tồn tại");
        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            wallet.Transactions.Add(new Transaction
            {
                Amount = amount,
                WalletId = wallet.Id,
                TransactionType = TransactionType.Deposit,
            });
            wallet.Amount += amount;
            walletRepo.Update(wallet);
        });

        await _invoiceService.AllocateMoneyToInvoices(customerId);
        return true;
    }

    public async Task<bool> WithdrawFromWallet(Guid customerId, int amount)
    {
        var walletRepo = _unitOfWork.GetRepository<Wallet>();
        var wallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId) ?? throw new NotFoundException($"Ví đã tồn tại");
        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            wallet.Transactions.Add(new Transaction
            {
                Amount = amount,
                WalletId = wallet.Id,
                TransactionType = TransactionType.Refund,
            });
            wallet.Amount -= amount;
            walletRepo.Update(wallet);
        });
        return true;
    }

    public async Task<bool> AddCreditToWallet(Guid customerId, int amount)
    {
        var walletRepo = _unitOfWork.GetRepository<Wallet>();
        var wallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId) ?? throw new NotFoundException($"Ví đã tồn tại");
        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            wallet.Transactions.Add(new Transaction
            {
                Amount = amount,
                WalletId = wallet.Id,
                TransactionType = TransactionType.Credit,
            });
            wallet.Amount += amount;
            walletRepo.Update(wallet);  
        });
        _unitOfWork.Context.ChangeTracker.Clear();
        await _invoiceService.AllocateMoneyToInvoices(customerId);
        return true;
    }

    public async Task<GetWalletResponse> CalculateWallet(Guid customerId)
    {
        var walletRepo = _unitOfWork.GetRepository<Wallet>();
        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();

        var wallet = await walletRepo.SingleOrDefaultAsync(
            predicate: w => w.CustomerId == customerId,
            include: w => w.Include(x => x.Transactions)
        );

        var invoices = await invoiceRepo.GetListAsync(predicate: i =>
            i.CustomerId == customerId &&
            !(i.IsDeleted ?? false) &&
            (i.InvoiceStatus == InvoiceStatus.Pending ||
             i.InvoiceStatus == InvoiceStatus.PartialPaid)
        );

        var totalDebt = invoices.Sum(i =>
        {
            var remaining = i.Total - i.DeductedAmount - i.PaidAmount;
            return Math.Max(0, remaining);
        });

        var result = _mapper.Map<GetWalletResponse>(wallet ?? new Wallet());

        result.TotalDebt = totalDebt;

        return result;
    }
}
