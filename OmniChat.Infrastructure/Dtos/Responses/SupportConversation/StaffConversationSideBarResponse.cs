using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.SupportConversation
{
    public class StaffConversationSideBarResponse
    {
        public Guid ConversationId { get; set; }
        
        public string CustomerName { get; set; } = null!;
        
        public string? AvartarUrl { get; set; }

        public string ProviderName { get;set; }

        public string LastMessage { get; set; }

        public int UnreadMessageCount { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
