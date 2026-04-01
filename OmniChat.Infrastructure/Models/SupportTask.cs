using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class SupportTask
    {
        public Guid Id { get; set; }

        public Guid SupportConversationId { get; set; }

        public virtual SupportConversation SupportConversation { get; set; }

        public Guid IntentTypeId { get; set; }

        public virtual IntentType IntentType { get; set; }

        public SupportTaskStatus Status { get; set; }

        public Guid? CurrentAssignedStaffId { get; set; }

        public virtual Staff? CurrentAssignedStaff { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompleteDate { get; set; }

        public int TaskPiority { get; set; }

        public virtual ICollection<TaskAction> TaskActions { get; set; } = new List<TaskAction>();

        public virtual ICollection<TaskCancelReason> TaskCancelReasons { get; set; } = new List<TaskCancelReason>();
    }

    public enum SupportTaskStatus
    {
        New = 0,
        InProgress = 1,
        PendingReassign = 2,
        Done = 3,
        Cancelled = 4,
        closed = 5,
    }
}
