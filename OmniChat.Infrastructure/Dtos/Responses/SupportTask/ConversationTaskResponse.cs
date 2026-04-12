using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.SupportTask
{
    public class ConversationTaskResponse
    {
        public Guid Id { get; set; }
        public string IntentTypeName { get; set; }
        public SupportTaskStatus Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
