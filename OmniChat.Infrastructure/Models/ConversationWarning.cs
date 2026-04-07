using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class ConversationWarning
    {
        public Guid Id { get; set; }

        public Guid ConversationId { get; set; }
        public virtual SupportConversation Conversation { get; set; }

        public Guid StaffId { get; set; }
        public virtual Staff Staff { get; set; }

        public WarningType WarningType { get; set; }   
        public string Reason { get; set; }              
        public bool IsReviewed { get; set; } = false;   
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum WarningType
    {
        TaskCompletedTooFast = 0,       
        ConversationClosedTooFast = 1,  
        BothFast = 2,
        StaffNotResponding = 3
    }
}
