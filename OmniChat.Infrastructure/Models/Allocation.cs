using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Allocation
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public virtual Wallet Wallet { get; set; }

        public Guid InvoiceId { get; set; }

        public virtual Invoice Invoice { get; set; }

        public double Amount { get; set; }

        public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    }
}
