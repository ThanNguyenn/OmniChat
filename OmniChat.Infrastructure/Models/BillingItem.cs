using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class BillingItem
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public virtual Order Order { get; set; }

        public Guid? PaymentId { get; set; }

        public virtual Payment? Payment { get; set; }

        public double Amount { get; set; }

        public BillingStatus BillStatus { get; set; }

        public DateTime CreateDate { get; set; }

    }

    public enum BillingStatus
    {
        Pending = 0,
        Completed = 1,
        Cancelled = 2,
    }
}
