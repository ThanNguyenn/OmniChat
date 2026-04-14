using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Staff
{
    public class ShipperResposne
    {
        public Guid Id { get; set; }

        public string ShipperName { get; set; }

        public StaffStatus ShipperStatus { get; set; }

        public string ShipperPhone { get; set; }

        public int TotalPendingOrders { get; set; }

        public int TotalOrderShipNow { get; set; }

        public int TotalOrderShipped { get; set; }
    }
}
