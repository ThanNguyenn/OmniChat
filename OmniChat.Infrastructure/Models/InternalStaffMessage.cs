using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class InternalStaffMessage
    {
        public Guid Id { get; set; }
        
        public Guid StaffId { get; set; }

        public virtual Staff Staff { get; set; }

        public InternalStaffMessageStatus Status { get; set; }

        public string Content { get; set; }

        public long Timestamp { get; set; }

        public Guid? InternalConversationId { get; set; }

        public virtual InternalConversation? InternalConversation { get; set; }
    }

    public enum InternalStaffMessageStatus
    {
        Pending = 0,
        Sent = 1
    }
}
