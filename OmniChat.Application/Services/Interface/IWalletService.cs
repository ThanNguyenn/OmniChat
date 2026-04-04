using Amazon.S3.Model;
using OmniChat.Infrastructure.Dtos.Requests.Wallet;
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

}
