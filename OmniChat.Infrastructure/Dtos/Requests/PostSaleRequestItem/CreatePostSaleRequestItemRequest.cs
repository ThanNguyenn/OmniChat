using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.PostSaleRequestItem;

public class CreatePostSaleRequestItemRequest
{
    public Guid OrderItemId { get; set; }

    public int Quantity { get; set; }
}
