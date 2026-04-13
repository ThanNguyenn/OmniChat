using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Invoice
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }

        public virtual CustomerProfile CustomerProfile { get; set; }

        public DateTime? StartedDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndedDate { get; set; }

        public double Total {  get; set; }

        public InvoiceStatus InvoiceStatus { get; set; }

        public InvoiceMethod InvoiceMethod { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime CreateAt { get; set; }

        public bool? IsDeleted { get; set; }

        public double PaidAmount { get; set; }

        public double DeductedAmount { get; set; }
        public virtual ICollection<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
    }

    public enum InvoiceStatus
    {
        Pending = 0,
        Completed = 1,
        Refunded = 2,
        PendingRefund = 3,
        Cancel = 4,
        PartialPaid = 5,
    }

    public enum InvoiceMethod
    {
        Cash = 0,
        BankTransfer = 1,
    }
}
