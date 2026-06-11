using Amazon.S3.Model;
using OmniChat.Infrastructure.Dtos.Requests.Wallet;
using OmniChat.Infrastructure.Dtos.Responses.Wallet;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IWalletService
{
    Task<bool> DeleteWallet(Guid customerId);
    Task<bool> CreateWallet(Guid customerId);
    Task <bool> DepositToWallet(WalletPaymentRequest walletPaymentRequest);
    Task<bool> WithdrawFromWallet(Guid customerId, int amount);
    Task<bool> AddCreditToWallet(Guid customerId, int amount);
    Task<GetWalletResponse> CalculateWallet(Guid customerId);
    Task<bool> HasDebt(Guid customerId);

    public  Task<GetCustomerWalletResponse> GetCustomerWallet(Guid customerId);
}
