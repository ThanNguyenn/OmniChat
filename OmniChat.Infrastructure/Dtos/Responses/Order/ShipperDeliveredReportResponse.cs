using OmniChat.Infrastructure.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Order
{
    public class ShipperDeliveredReportResponse
    {
        public int TotalDeliveredOrders { get; set; }
        public PagingResponse<GetOrderResponse> Orders { get; set; }
    }
}
