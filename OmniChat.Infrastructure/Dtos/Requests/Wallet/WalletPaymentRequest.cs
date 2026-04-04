using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Wallet;

public class WalletPaymentRequest
{
    public Guid CustomerId { get; set; }
    public int Amount { get; set; }
}
