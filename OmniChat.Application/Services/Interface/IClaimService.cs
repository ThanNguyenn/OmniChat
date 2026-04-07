using OmniChat.Infrastructure.Dtos.Requests.Claim;
using OmniChat.Infrastructure.Dtos.Responses.Claim;
using OmniChat.Infrastructure.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IClaimService
    {
        public  Task<bool> CreateClaimAsync(CreateClaimRequest claimRequest);

        public  Task<ClaimDashboardResponse> GetClaimDashboardAsync();

        public  Task<PagingResponse<ClaimDetailResponse>> GetPendingClaimAsync(int pageIndex = 1, int pageSize = 10);

        public Task<PagingResponse<ClaimDetailResponse>> GetClaimHistoryAsync(int pageIndex = 1, int pageSize = 10);

        public  Task<IEnumerable<StaffClaimResponse>> GetClaimsByStaffIdAsync(Guid staffId);

        public  Task<bool> UpdateClaimInforAsync(Guid claimId, UpdateClaimRequest claimRequest);

        public  Task<ClaimDetailResponse> ApproveClaimAsync(Guid claimId);

        public  Task<ClaimDetailResponse> RejectClaimAsync(Guid claimId);

        public  Task ReAssignStaffAsync(Guid newStaffAssignId, Guid conversationReAssignId);
    }
}
