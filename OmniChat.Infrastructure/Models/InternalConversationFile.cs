using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class InternalConversationFile
    {
        public Guid Id { get; set; }
      
        public Guid ConversationFileId { get; set; }

        public virtual ConversationFile ConversationFile { get; set; }

        public Guid InternalConversationId { get; set; }

        public virtual InternalConversation InternalConversation { get; set; }
    }
}
