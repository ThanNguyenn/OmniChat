using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class TaskAssignments
    {
        public Guid Id { get; set; }

        public Guid DepartmentId { get; set; }

        public virtual Department Department { get; set; }

        public Guid SupportConversationId { get; set; }

        public virtual SupportConversation SupportConversation { get; set; }

        public DateTime CreateDate { get; set; }

        public TaskAssignmentStatus Status { get; set; }

        public Guid? AssignedBy { get; set; }

        public AssignedType AssignedType { get; set; }

        public bool? IsActive { get; set; }

        public Guid StaffId { get; set; }

        public virtual Staff Staff { get; set; }
    }

    public enum TaskAssignmentStatus
    {
        Pending = 0,
        Complete = 1,
    }

    public enum AssignedType
    {
        Auto = 0,
        Manual = 1,
    }
}
