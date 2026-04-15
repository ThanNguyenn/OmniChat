using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Wallet;

public class GetWalletResponse
{
    public double WalletAmount { get; set; }

    public double TotalDebt { get; set; }
}
