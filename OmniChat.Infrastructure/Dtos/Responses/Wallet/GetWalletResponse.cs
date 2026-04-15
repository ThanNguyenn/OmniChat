using OmniChat.Infrastructure.Dtos.Responses.Transaction;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Wallet;

public class GetWalletResponse
{
    public double Amount { get; set; }

    public double TotalDebt { get; set; }

    public double NetAmount => Amount - TotalDebt;

    public IEnumerable<GetTransactionResponse> Transactions { get; set; }
}
