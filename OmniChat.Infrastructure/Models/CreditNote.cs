using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class CreditNote
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public virtual Order Order { get; set; }

        public Guid? InvoiceId { get; set; }

        public virtual Invoice? Invoice { get; set; }

        public double Total { get; set; }

        public CreditNoteStatus CreditNoteStatus { get; set; }

        public DateTime? CreateDate { get; set; } = DateTime.UtcNow;

        public CreditNoteType CreditNoteType { get; set; }
    }

    public enum CreditNoteType
    {
        Refund = 0,
        Adjustment = 1,
    }

    public enum CreditNoteStatus
    {
        Pending = 0,
        Completed = 1,
        Cancelled = 2,
    }
}
