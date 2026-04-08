using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.OrderItem;

public class GetOrderItemResponse
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }

    public string ProductName { get; set; }

    public string ItemsPrice { get; set; }

}
