using OmniChat.Infrastructure.Dtos.Responses.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Wallet
{
    public class GetCustomerWalletResponse
    {
        public double Amount { get; set; }

        public double TotalDebt { get; set; }

        public IEnumerable<GetCustomerTransactionResponse> CustomerTransactions { get; set; }
    }
}
