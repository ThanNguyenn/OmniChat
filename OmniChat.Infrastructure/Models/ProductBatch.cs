using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class ProductBatch
    {
        public Guid Id { get; set; }
        
        public Guid ProductId { get; set; }

        public virtual Product Product { get; set; }

        public DateTime? ManuFactureDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiryDate { get; set; } = DateTime.UtcNow;

        public int Quantity { get; set; } 

        public bool? IsActive { get; set; }

        public string Code { get; set; } 

        public bool? IsExpired { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public virtual ICollection<BatchAudit> BatchAudits { get; set; } = new List<BatchAudit>();
    }
}
