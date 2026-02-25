using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.SupportConversation
{
    public class CompleteSupportConversationHistoryResponse
    {
        public string ProviderName { get; set; } // Get provide Name 

        public ConversationStatus Status { get; set; } // CompleteConversation

        public DateTime CompleteDate { get; set; } // Task complete date

        public string KeywordType { get; set; } // Keyword type Name of Task

        public string StaffName { get; set; } // Staff Name
    }
}
