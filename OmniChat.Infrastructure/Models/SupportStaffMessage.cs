using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class SupportStaffMessage
    {
        public Guid Id { get; set; }

        public Guid SupportConversationId { get; set; }

        public virtual SupportConversation SupportConversation { get; set; }

        public Guid StaffId { get; set; }

        public virtual Staff Staff { get; set; }

        public string Content { get; set; }

        public SupportStaffMessageStatus Status { get; set; }

        public long Timestamp { get; set; }


    }

    public enum SupportStaffMessageStatus
    {
        Pending = 0,
        Sent = 1,
    }
}
