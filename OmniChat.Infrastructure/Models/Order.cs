using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid CustomerId  { get; set; }

        public virtual CustomerProfile CustomerProfile { get; set; }

        public Guid? InvoiceId { get; set; }

        public virtual Invoice Invoice { get; set; }

        public DateTime? OrderDate { get; set; } = DateTime.UtcNow;

        public string Name { get; set; }

        public OrderStatus Status { get; set; }

        public double TotalAmount { get; set; }

        public DeliveryStatus? DeliveryStatus { get; set; }

        public string Code { get; set; }

        public bool? IsDeleted { get; set; }

        public Guid? DriverId { get; set; }

        public virtual Staff Driver { get; set; }

        public Guid CreatorId { get; set; }
       
        public virtual Staff Creator { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public virtual ICollection<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();

        public virtual ICollection<PostSaleRequest> PostSaleRequests { get; set; } = new List<PostSaleRequest>();
    }

    public enum OrderStatus
    {
        Draft = 0,
        Pending = 1,
        Cancelled = 2,
        Shipped = 3,
        PendingReturn = 4,
        Returned = 5,
        Completed = 6,
        ReturnedDefective = 7,
    }

    public enum DeliveryStatus
    {
       Pending = 0,
       Completed = 1,
    }
}
