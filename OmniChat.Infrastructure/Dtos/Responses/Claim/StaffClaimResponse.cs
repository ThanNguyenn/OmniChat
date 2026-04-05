using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Claim
{
    public class StaffClaimResponse
    {
        public string ClaimType { get; set; }

        public DateTime SubmitDate { get; set; }

        public ClaimStatus Status { get; set; }

        public string Description { get; set; }

        public string Reason { get; set; }

    }
}
