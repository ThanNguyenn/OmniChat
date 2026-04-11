using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Order;

public class DashboardOrderYearResponse
{
    public string Month { get; set; }
    public IEnumerable<GetOrderDashBoardByStatus> Status { get; set; }
}
