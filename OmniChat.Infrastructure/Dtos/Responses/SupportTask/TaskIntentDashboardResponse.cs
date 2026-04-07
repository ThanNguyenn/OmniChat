using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.SupportTask;

public class TaskIntentDashboardResponse
{
    public string IntentName { get; set; }

    public int TaskCount { get; set; }
}
