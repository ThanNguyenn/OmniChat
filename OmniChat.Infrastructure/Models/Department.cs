using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Department
    {
        public Guid Id { get; set; }
        
        public string DepartmentName { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public virtual ICollection<Staff> Staffs { get; set; } = new List<Staff>();
    
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

        public virtual ICollection<Kpi> Kpis { get; set; } = new List<Kpi>();

        public virtual ICollection<DepartmentKeyword> DepartmentKeywords { get; set; } = new List<DepartmentKeyword>();

        public virtual ICollection<DepartmentConversation> DepartmentConversations { get; set; } = new List<DepartmentConversation>();
    
        public virtual ICollection<TaskAssignments> TaskAssignments { get; set; } = new List<TaskAssignments>();
    }
}
