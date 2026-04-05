using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Staff
{
    public class StaffDassboardResponse
    {
        public int TotalDoneTask { get; set; }

        public int TotalCreateOrder { get; set; }

        public double AfferageResolveTime { get; set; }

        public double StaffPerformance { get; set; }

    }
}
