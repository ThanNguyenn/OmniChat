using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.CustomerMessage
{
    public class GetAllCustomerMessageResponse
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public long Timestamp { get; set; }

        public bool KeywordActive { get; set; }

        public Guid CustomerId { get; set; }

        public Guid ConversationId { get; set; }
    }
}
