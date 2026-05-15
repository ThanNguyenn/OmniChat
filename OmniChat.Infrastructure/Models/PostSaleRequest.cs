using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class PostSaleRequest
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; } // the customer who present the request

        public virtual CustomerProfile Customer { get; set; }

        public Guid OrderId { get; set; } // the order that the request is related to

        public virtual Order Order { get; set; }

        public Guid PresentByStaffId { get; set; } // staff who present the request

        public virtual Staff PresentByStaff { get; set; }

        public PostSaleRequestType Type { get; set; }

        public bool? FraudFlag { get; set; } // default false

        public Guid? ResolveById { get; set; } // staff(manager) who resolve the request 

        public virtual Staff? ResolveBy { get; set; }

        public PostSaleRequestStatus Status { get; set; }

        public string Reason { get; set; }

        public double? RefundAmount { get; set; } // only for return and refund request

        public DateTime? RequestedTime { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedTime { get; set; } = DateTime.UtcNow;

        public DateTime? CreateTime { get; set; } = DateTime.UtcNow; 

        public virtual ICollection<PostSaleItem>? PostSaleItems { get; set; }

    }

    public enum PostSaleRequestType
    {
       //Cancel = 0,
       Return = 1,
       //Replacement = 2,
       Refund = 3,  
    }

    public enum PostSaleRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
    }
}
