using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class CustomerMessage
    {
        public Guid Id { get; set; }
       public string Content { get; set; }

        public long Timestamp { get; set; }

        public bool KeywordActive { get; set; }

        public Guid CustomerId { get; set; }

        public bool? IsRead { get; set; } 

        public virtual CustomerProfile Customer { get; set; }

        public Guid ConversationId { get; set; }

        public virtual SupportConversation Conversation { get; set; }

        public virtual ICollection<MessageKeywordTypes> MessageKeywordTypes { get; set; } = new List<MessageKeywordTypes>();

    }
}
