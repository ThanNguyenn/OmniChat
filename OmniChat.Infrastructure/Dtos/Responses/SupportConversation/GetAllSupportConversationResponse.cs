using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.SupportConversation
{
    public class GetAllSupportConversationResponse
    {
        public Guid Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public ConversationStatus Status { get; set; }

        public bool IsDistributed { get; set; }

        public string CustomerName { get; set; }

        public string AvartarUrl { get; set; }

        public Guid? ActiveStaffId { get; set; }

        public Guid ActiveCustomerId { get; set; }

        public Guid ProvidersId { get; set; }
    }
}
