using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Claim
{
    public class ClaimDetailResponse
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string Reason { get; set; }

        public DateTime SubmitDate { get; set; }

        public ClaimStatus Status { get; set; }

        public Guid StaffId { get; set; }

        public string StaffName { get; set; }

        public Guid ClaimTypeId { get; set; }

        public string ClaimTypeName { get; set; }
    }
}
