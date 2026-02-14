using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Claim
{
    public class UpdateClaimRequest
    {
        public string Description { get; set; }

        public string Reason { get; set; }

        public Guid ClaimTypeId { get; set; }
    }
}
