using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.OrderItem;

public class UpdateOrderItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity phải >= 1")]
    public int Quantity { get; set; }
}
