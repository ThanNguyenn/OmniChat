using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class StaffKpi
    {
        public Guid Id { get; set; }
        
        public StaffKpiStatus Status { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public Guid StaffId { get; set; }

        public virtual Staff Staff { get; set; }

        public Guid KpiId { get; set; }

        public virtual Kpi Kpi { get; set; }

        public int CurrentValue { get; set; }
    }

    public enum StaffKpiStatus
    {
       Done = 0,
       Pending = 1,
       Faild = 2,
    }
}
