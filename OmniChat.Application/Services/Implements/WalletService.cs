using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Allocation;
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
        var wallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId) ?? throw new NotFoundException($"Ví không tồn tại");

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
        var wallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId) ?? throw new NotFoundException($"Ví không tồn tại");
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
        var wallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId) ?? throw new NotFoundException($"Ví không tồn tại");
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
        var wallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.CustomerId == customerId) ?? throw new NotFoundException($"Ví không tồn tại");
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
            include: w => w.Include(x => x.Transactions.OrderByDescending(t => t.CreateDate))
        );

        var invoices = await invoiceRepo.GetListAsync(predicate: i =>
            i.CustomerId == customerId &&
            !(i.IsDeleted ?? false) &&
            (i.InvoiceStatus == InvoiceStatus.Pending ||
             i.InvoiceStatus == InvoiceStatus.PartialPaid)
        );

        var totalDebt = invoices.Sum(i =>
        {
            var remaining = i.Total - i.PaidAmount;
            return Math.Max(0, remaining);
        });

        var result = _mapper.Map<GetWalletResponse>(wallet ?? new Wallet());

        result.TotalDebt = totalDebt;

        return result;
    }

    public async Task<bool> HasDebt(Guid customerId)
    {
        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();

        var invoices = await invoiceRepo.GetListAsync(predicate: i =>
            i.CustomerId == customerId &&
            !(i.IsDeleted ?? false) &&
            (i.InvoiceStatus == InvoiceStatus.Pending ||
             i.InvoiceStatus == InvoiceStatus.PartialPaid)
        );

        var totalDebt = invoices.Sum(i =>
        {
            var remaining = i.Total - i.PaidAmount;
            return remaining > 0 ? remaining : 0;
        });

        return totalDebt > 0;
    }


    public async Task<GetCustomerWalletResponse> GetCustomerWallet(Guid customerId)
    {
        var walletRepo = _unitOfWork.GetRepository<Wallet>();


        var customerWallet = await walletRepo.SingleOrDefaultAsync(predicate:
            w => w.CustomerId == customerId,
            include: w =>
                          w.Include(x => x.Transactions.OrderByDescending(t => t.CreateDate))
                           .Include(w => w.Allocations.OrderByDescending(a => a.CreateDate)).ThenInclude(a => a.Invoice)
            );

        var response = _mapper.Map<GetCustomerWalletResponse>(customerWallet);

        var invoiceCaculate = await CalculateWallet(customerId);

        response.TotalDebt = invoiceCaculate.TotalDebt;

        return response;
    }

    public async Task<bool> AllocationMoneyToInvoice(Guid invoiceId, AllocationWalletMoneyRequest request)
    {
        var walletRepo = _unitOfWork.GetRepository<Wallet>();

        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();

        var customerWallet = await walletRepo.SingleOrDefaultAsync(predicate: w => w.Id == request.WalletId)
            ?? throw new NotFoundException($"Ví không tồn tại");

        var invoice = await invoiceRepo.SingleOrDefaultAsync(predicate: i => i.Id == invoiceId)
            ?? throw new NotFoundException($"Hóa đơn không tồn tại");

        if (invoice.InvoiceStatus == InvoiceStatus.Completed)
        {
            throw new BusinessException($"Hóa đơn đã được thanh toán hoàn , vui lòng chọn hóa đơn khác.");
        }

        if (request.deductedAmount > customerWallet.Amount)
        {
            throw new BusinessException($"Số dư trong ví không đủ để chi trả");
        }

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            if (customerWallet.Amount >= request.deductedAmount)
            {

                var paidAmount = invoice.Total - request.deductedAmount;

                // final amount to pay for invoice
                invoice.PaidAmount = paidAmount;

                // deduct money from wallet
                customerWallet.Amount -= request.deductedAmount;

                walletRepo.Update(customerWallet);
                invoiceRepo.Update(invoice);
            }
        });
        return true;
    }

}
