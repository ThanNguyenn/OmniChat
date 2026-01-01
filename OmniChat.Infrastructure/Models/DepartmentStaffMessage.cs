using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class DepartmentStaffMessage
    {
        public Guid Id { get; set; }

        public Guid StaffId { get; set; }

        public virtual Staff Staff { get; set; }

        public string Content { get; set; }

        public DepartmentStaffMessageStatus Status { get; set; }

        public long Timestamp { get; set; }

        public Guid DepartmentConversationId { get; set; }

        public virtual DepartmentConversation DepartmentConversation { get; set; }
    }

    public enum DepartmentStaffMessageStatus
    {
        Pending = 0,
        Sent = 1,
    }
}
