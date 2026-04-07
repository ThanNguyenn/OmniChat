using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Claim
{
    public class ClaimDashboardResponse
    {
        public int PendingClaims { get; set; }

        public int ApprovedClaims { get; set; }

        public int RejectedClaims { get; set; }
    }
}
