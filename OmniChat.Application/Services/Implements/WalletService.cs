using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Wallet;
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
            throw new BusinessException($"Wallet for customer {customerId} already exists");
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
        var wallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId) ?? throw new NotFoundException($"Wallet for customer {customerId} do not exists");

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
        var wallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId) ?? throw new NotFoundException($"Wallet for customer {customerId} do not exists");
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
        return true;
    }

    public async Task<bool> WithdrawFromWallet(Guid customerId, int amount)
    {
        var walletRepo = _unitOfWork.GetRepository<Wallet>();
        var wallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId) ?? throw new NotFoundException($"Wallet for customer {customerId} do not exists");
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
        var wallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId) ?? throw new NotFoundException($"Wallet for customer {customerId} do not exists");
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
        _ = _invoiceService.AllocateMoneyToInvoices(customerId);
        return true;
    }
}
