using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Staff
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public StaffStatus Status { get; set; }

        public bool IsActive { get; set; }

        public Guid AccountId { get; set; }

        public virtual Account Account { get; set; }

        public Guid DepartmentId { get; set; }

        public virtual Department Department { get; set; }

        public virtual ICollection<SupportConversation> SupportConversations { get; set; } = new List<SupportConversation>();

        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

        public virtual ICollection<StaffShift> StaffShifts { get; set; } = new List<StaffShift>();

        public virtual ICollection<StaffKpi> StaffKpis { get; set; } = new List<StaffKpi>();

        public virtual ICollection<FeedBack> FeedBacks { get; set; } = new List<FeedBack>();

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    
        public virtual ICollection<SupportStaffMessage> SupportStaffMessages { get; set; } = new List<SupportStaffMessage>();

        public virtual ICollection<DepartmentStaffMessage> DepartmentStaffMessages { get; set; } = new List<DepartmentStaffMessage>();

        public virtual ICollection<TaskAssignments> TaskAssignments { get; set; } = new List<TaskAssignments>();
    }

    public enum StaffStatus
    {
        Online = 0,
        Offline = 1,
    }
}
