using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.ClaimType
{
    public class GetClaimTypeResponse
    {
        public Guid Id { get; set; }

        public string TypeName { get; set; }
    }
}
