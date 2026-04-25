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

        public Task<PagingResponse<ClaimDetailResponse>> GetClaimsByStaffIdAsync(Guid staffId, int pageIndex = 1, int pageSize = 10);

        public  Task<bool> UpdateClaimInforAsync(Guid claimId, UpdateClaimRequest claimRequest);

        public  Task<ClaimDetailResponse> ApproveClaimAsync(Guid claimId);

        public  Task<ClaimDetailResponse> RejectClaimAsync(Guid claimId);

        public  Task ReAssignStaffAsync(Guid claimId, Guid newStaffAssignId, Guid conversationReAssignId);

        public  Task RejectReassignClaimAsync(Guid claimId, Guid ManagerId);

        public  Task<PagingResponse<ClaimDetailResponse>> GetPendingChangeTask(int pageIndex = 1, int pageSize = 10);
    }
}
