using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.SupportConversation
{
    public class SupportConversationMessagesResponse
    {
        public string SenderType { get; set; }

        public Guid SenderId { get; set; } // staff Id or customer profile Id

        public string Content { get; set; }

        public long Timestamp { get; set; }
    }
}
