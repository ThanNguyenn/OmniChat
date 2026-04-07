using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Order;

public class GetOrderDashBoardByStatus
{
    public OrderStatus Status { get; set; }
    public int Count { get; set; }
}
