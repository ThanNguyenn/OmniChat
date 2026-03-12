using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Wallet
    {
        public Guid Id { get; set; }
        
        public Guid CustomerId { get; set; }
        
        public virtual CustomerProfile CustomerProfile { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; } = DateTime.UtcNow;

        public double Amount { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
    }
}
