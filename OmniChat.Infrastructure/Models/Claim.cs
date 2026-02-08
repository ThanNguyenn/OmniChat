using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Claim
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string Reason { get; set; }

        public DateTime SubmitDate { get; set; }

        public ClaimStatus Status { get; set; }

        public Guid KeywordTypeId { get; set; }

        public virtual KeywordTypes KeywordTypes { get; set; }

        public Guid StaffId { get; set; }

        public virtual Staff Staff { get; set; }

        public Guid ClaimTypeId { get; set; }

        public virtual ClaimType ClaimType { get; set; }
    }

    public enum ClaimStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
    }
}
