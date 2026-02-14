using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Order;

public class UpdateOrderDeliveryStatusRequest
{
    public DeliveryStatus NewDeliveryStatus { get; set; }
}
