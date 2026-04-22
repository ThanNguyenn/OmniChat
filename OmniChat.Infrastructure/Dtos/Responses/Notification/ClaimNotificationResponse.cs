using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Notification
{
    public class ClaimNotificationResponse
    {
        public string ConversationName { get; set; }

        public string Description { get; set; }

        public ClaimStatus Status { get; set; }

        public string NewStatus { get; set; }

        public string Message { get; set; }
    }
}
