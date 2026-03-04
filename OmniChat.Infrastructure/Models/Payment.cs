using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Payment
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }

        public virtual CustomerProfile CustomerProfile { get; set; }

        public DateTime StartedDate { get; set; }

        public DateTime EndedDate { get; set; }

        public double Total {  get; set; }

        public PaymentStatus PayStatus { get; set; }

        public PaymentMethod PayMethod { get; set; }

        public DateTime CompletedDate { get; set; }

        public DateTime CreateAt { get; set; }

        public virtual ICollection<BillingItem> BillingItems { get; set; } = new List<BillingItem>();
    }

    public enum PaymentStatus 
    {
        Pending = 0,
        Completed = 1,
        Refunded = 2,
        PendingRefund = 3,
        Cancel = 4,
        PartialPaid = 5,
    }

    public enum PaymentMethod
    {
        Cash = 0,
        BankTransfer = 1,
    }
}
