using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Claim
{
    public class CreateClaimRequest
    {
        public string Description { get; set; }

        public string Reason { get; set; }

        public Guid StaffId { get; set; }

        public Guid ClaimTypeId { get; set; }

        public Guid? SupportConversationId { get; set; }
    }
}
