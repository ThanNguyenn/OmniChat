using OmniChat.Infrastructure.Dtos.Requests.ClaimType;
using OmniChat.Infrastructure.Dtos.Responses.ClaimType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IClaimTypeService
    {
        public  Task<bool> CreateNewClaimTypeAsync(ClaimTypeRequest typeRequest);

        public  Task<IEnumerable<GetClaimTypeResponse>> GetAllTypeAsync();

        public  Task<bool> UpdateClaimTypeAsync(Guid claimTypeId, ClaimTypeRequest typeRequest);

        public  Task<bool> DeleteClaimTypeByIdAsync(Guid claimTypeId);

    }
}
