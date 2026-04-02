
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class PostSaleItem
    {
        public Guid Id { get; set; }

        public Guid PostSaleRequestId { get; set; }

        public virtual PostSaleRequest PostSaleRequest { get; set; }

        public Guid OrderItemId { get; set; }

        public virtual OrderItem OrderItem { get; set; }

        public int Quantity { get; set; }
    }
}
