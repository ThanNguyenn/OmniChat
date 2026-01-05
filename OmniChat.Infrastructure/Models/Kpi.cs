using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Kpi
    {
        public Guid Id { get; set; }

        public string name { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public int TargetValue { get; set; }

        public int AchivedValue { get; set; }

        public bool? IsDeleted { get; set; }

        public Guid DepartmentId { get; set; }

        public virtual Department Department { get; set; }

        public virtual ICollection<StaffKpi> StaffKpis { get; set; } = new List<StaffKpi>();
    }
}
