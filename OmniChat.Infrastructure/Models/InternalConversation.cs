using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class InternalConversation
    {
        public Guid Id { get; set; }

        public string ConversationName { get; set; }

        public bool? IsActive { get; set; }

        public InternalConversationStatus Status { get; set; }

        public DateTime? CreateDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<InternalConversationFile> InternalConversationFiles { get; set; } = new List<InternalConversationFile>();
        
        public virtual ICollection<InternalStaffMessage> InternalMessages { get; set; } = new List<InternalStaffMessage>();
    }

    public enum InternalConversationStatus
    {
        Online = 0,
        Offline = 1,
    }
}
