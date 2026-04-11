using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.FeedBack
{
    public class FeedBackResponse
    {
        public Guid Id { get; set; }

        public Guid StaffId { get; set; }

        public string StaffName { get; set; }

        public Guid SupportConversationId { get; set; }

        public string Content { get; set; }

        public string CustomerEmail { get; set; }

        public int Rating { get; set; }

        public string FormUrl { get; set; }
    }
}
