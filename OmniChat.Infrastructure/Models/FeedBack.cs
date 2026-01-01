using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class FeedBack
    {
        public Guid Id { get; set; }

        public Guid StaffId { get; set; }

        public virtual Staff Staff { get; set; }

        public Guid SupportConversationId { get; set; }

        public virtual SupportConversation SupportConversation { get; set; }

        public string Content { get; set; }

        public string CustomerEmail { get; set; }

        public int Rating { get; set; }

        public string FormUrl { get; set; }


    }
}
