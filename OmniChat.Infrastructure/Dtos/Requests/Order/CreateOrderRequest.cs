using OmniChat.Infrastructure.Dtos.Requests.OrderItem;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Order;

public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }

    public string Name { get; set; }

    public IEnumerable<AddOrderItemRequest> OrderItems { get; set; }

}
