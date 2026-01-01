using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Notification
    {
        public Guid Id { get; set; }

        public string MessageText { get; set; }

        public Guid? StaffId { get; set; }
        
        public virtual Staff? Staff { get; set; }

        public Guid ConversationId { get; set; }

        public virtual SupportConversation SupportConversation { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsRead { get; set; }
    }
}
