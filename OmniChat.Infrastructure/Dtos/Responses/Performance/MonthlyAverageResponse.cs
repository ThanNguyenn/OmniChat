using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Performance
{
    public class MonthlyAverageResponse
    {
        public int Year { get; set; }
        public IEnumerable<MonthlyAverageItem> MonthlyData { get; set; }
    }
}
