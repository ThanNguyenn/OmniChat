using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Performance
{
    public class MonthlyAverageItem
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public double AverageTotalResponseTime { get; set; }
        public int TotalCustomerMessages { get; set; }
        public double TotalAverageTaskComplete { get; set; }
        public double TotalAverageCompleteConversation { get; set; }
    }
}
