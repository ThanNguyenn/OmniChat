using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Claim;
using OmniChat.Infrastructure.Dtos.Responses.Claim;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class ClaimService : BaseService<ClaimService>, IClaimService
    {
        public ClaimService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<ClaimService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<bool> CreateClaimAsync(CreateClaimRequest claimRequest)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
             {
                 // call repo 
                 var _repo = _unitOfWork.GetRepository<Claim>();

                 // map entity

                 var entity = _mapper.Map<Claim>(claimRequest);

                 // Insert Database

                 await _repo.InsertAsync(entity);

                 return true;
             });
        }


        public async Task<IEnumerable<ClaimDetailResponse>> GetAllClaim()
        {
            var _repo = _unitOfWork.GetRepository<Claim>();

            var claims = await _repo.GetListAsync();

            return _mapper.Map<IEnumerable<ClaimDetailResponse>>(claims);
        }

        public async Task<bool> UpdateClaimInforAsync(Guid claimId, UpdateClaimRequest claimRequest)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                if (claimRequest == null)
                    throw new BadRequestException("New Claim is invalid");

                var claim = await GetPendingClaimAsync(claimId);

                _mapper.Map(claimRequest, claim);

                _unitOfWork.GetRepository<Claim>().Update(claim);

                return true;
            });
        }

        private async Task<Claim> GetPendingClaimAsync(Guid claimId)
        {
            var _repo = _unitOfWork.GetRepository<Claim>();

            var claim = await _repo.SingleOrDefaultAsync(predicate: c => c.Id == claimId);

            if (claim == null)
                throw new NotFoundException("Claim not found");

            if (claim.Status != ClaimStatus.Pending)
                throw new BadRequestException("Only pending claim can be modified");

            return claim;
        }

        public async Task<ClaimDetailResponse> ApproveClaimAsync(Guid claimId)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var claim = await GetPendingClaimAsync(claimId);

                claim.Status = ClaimStatus.Approved;

                _unitOfWork.GetRepository<Claim>().Update(claim);

                return _mapper.Map<ClaimDetailResponse>(claim);
            });
        }


        public async Task<ClaimDetailResponse> RejectClaimAsync(Guid claimId)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var claim = await GetPendingClaimAsync(claimId);

                claim.Status = ClaimStatus.Rejected;

                _unitOfWork.GetRepository<Claim>().Update(claim);

                return _mapper.Map<ClaimDetailResponse>(claim);
            });
        }

        public async Task<IEnumerable<StaffClaimResponse>> GetClaimsByStaffIdAsync(Guid staffId)
        {
            var repo = _unitOfWork.GetRepository<Claim>();

            var claims = await repo.GetListAsync(
                predicate: x => x.StaffId == staffId,
                include: q => q.Include(x => x.ClaimType),
                orderBy: q => q.OrderByDescending(x => x.SubmitDate)
            );

            if (!claims.Any())
                throw new NotFoundException("No claims found for this staff");

            return _mapper.Map<IEnumerable<StaffClaimResponse>>(claims);
        }
    }
}
