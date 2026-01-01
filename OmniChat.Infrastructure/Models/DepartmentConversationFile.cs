using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class DepartmentConversationFile
    {
        public Guid Id { get; set; }

        public Guid DepartmentConversationId { get; set; }

        public virtual DepartmentConversation DepartmentConversation { get; set; }

        public Guid ConversationFileId  { get; set; }

        public virtual ConversationFile ConversationFile { get; set; }
    }
}
