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

        public DateTime ManuFactureDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public int Quantity { get; set; }

        public bool? IsActive { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
