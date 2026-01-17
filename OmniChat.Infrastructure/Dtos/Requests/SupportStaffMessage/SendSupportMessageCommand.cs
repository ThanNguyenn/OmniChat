using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage
{
    public class SendSupportMessageCommand
    {
        public Guid SupportConversationId { get; set; }
        public Guid StaffId { get; set; }
        public string Content { get; set; } = null!;
        public string Provider { get; set; } = null!; // "Facebook" | "Instagram"
    }
}
