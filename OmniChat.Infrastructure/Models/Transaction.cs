using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public virtual Wallet Wallet { get; set; }

        public double Amount { get; set; }

        public DateTime? CreateDate { get; set; } = DateTime.UtcNow;

        public TransactionType TransactionType { get; set; }
    }

    public enum TransactionType
    {
        Credit = 0,
        Deposit = 1,
        Refund = 2,
    }
}
