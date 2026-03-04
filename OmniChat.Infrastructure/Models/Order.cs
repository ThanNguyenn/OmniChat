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

        public DateTime OrderDate { get; set; }

        public string Name { get; set; }

        public OrderStatus Status { get; set; }

        public double TotalAmount { get; set; }

        public DeliveryStatus DeliveryStatus { get; set; }

        public string Code { get; set; }

        public bool? IsDeleted { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public virtual ICollection<BillingItem> BillingItems { get; set; } = new List<BillingItem>();
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
    }

    public enum DeliveryStatus
    {
       Pending = 0,
       Completed = 1,
    }
}
