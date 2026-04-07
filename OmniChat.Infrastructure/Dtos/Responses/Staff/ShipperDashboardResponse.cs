using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Staff
{
    public class ShipperDashboardResponse
    {
        public int ActiveShippers { get; set; }

        public int DeliveringOrders { get; set; }

        public int DeliveredToday { get; set; }
    }
}
