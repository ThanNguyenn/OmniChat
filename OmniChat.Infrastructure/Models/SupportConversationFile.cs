using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class SupportConversationFile
    {
        public Guid Id { get; set; }

        public Guid SupportConversationId { get; set; }

        public virtual SupportConversation SupportConversation { get; set; }

        public Guid ConversationFileId { get; set; }

        public virtual ConversationFile ConversationFile { get; set; }

    }
}
