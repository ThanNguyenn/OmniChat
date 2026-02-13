using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class OrderItem
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public virtual Order Order { get; set; }

        public int Quantity { get; set; }

        public double Price { get; set; }

        public Guid ProductBatchId { get; set; }

        public virtual ProductBatch ProductBatch { get; set; }
    }
}
