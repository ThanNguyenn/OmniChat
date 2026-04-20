using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.SupportConversation
{
    public class StaffConversationResponse
    {
        public Guid ConversationId { get; set; }
        public string customerName { get; set; }
    }
}
