using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.OrderItem;

public class AddOrderItemRequest
{
    public Guid ProductBatchId { get; set; }
    public int Quantity { get; set; }
}
