using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.SupportTask;

public class DashboardMonthResponse
{
    public int Month { get; set; }
    public IEnumerable<TaskIntentDashboardResponse> Intents { get; set; }
}
