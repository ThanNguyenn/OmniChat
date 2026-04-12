using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Notification
{
    public class NotificationRequest
    {
        public string MessageText { get; set; }

        public Guid? StaffId { get; set; }

        public Guid ConversationId { get; set; }

        public bool IsRead { get; set; }
    }
}
