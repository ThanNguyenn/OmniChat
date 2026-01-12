using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.ClaimType;
using OmniChat.Infrastructure.Dtos.Responses.ClaimType;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class ClaimTypeService : BaseService<ClaimTypeService>, IClaimTypeService
    {
        public ClaimTypeService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<ClaimTypeService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<bool> CreateNewClaimTypeAsync(ClaimTypeRequest typeRequest)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {

                // Call repo
                var _repo = _unitOfWork.GetRepository<ClaimType>();

                // check duplicate
                var existed = await _repo.SingleOrDefaultAsync(predicate: x => x.TypeName == typeRequest.TypeName && x.IsActive == true);

                if (existed != null)
                {
                    throw new BadRequestException("ClaimType already exists");
                }
                // Map request to entity

                var entity = _mapper.Map<ClaimType>(typeRequest);

                // Insert database

                await _repo.InsertAsync(entity);

                return true;
            });

        }

        public async Task<IEnumerable<GetClaimTypeResponse>> GetAllTypeAsync()
        {
            var repo = _unitOfWork.GetRepository<ClaimType>();

            var claimTypes = await repo.GetListAsync(predicate: x => x.IsActive == true);

            return _mapper.Map<IEnumerable<GetClaimTypeResponse>>(claimTypes);
        }

        public async Task<bool> UpdateClaimTypeAsync(Guid claimTypeId, ClaimTypeRequest typeRequest)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                // Call Repo
                var _repo = _unitOfWork.GetRepository<ClaimType>();

                // get by id

                var existClaimType = await _repo.GetByIdAsync(claimTypeId);

                if(existClaimType == null || existClaimType.IsActive == false)
                {
                    throw new NotFoundException("ClaimType not found");
                }

                // check null
                if (typeRequest == null)
                {
                    throw new BadRequestException("New ClaimType is invalid");
                }

                // map newClaim type -> exit claim type
                _mapper.Map(typeRequest, existClaimType);

                _repo.Update(existClaimType);
                return true;
            });
        }

        public async Task<bool> DeleteClaimTypeByIdAsync(Guid claimTypeId)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                // call repo
                var _repo = _unitOfWork.GetRepository<ClaimType>();

                var existClaimType = await _repo.GetByIdAsync(claimTypeId);

                if (existClaimType == null || existClaimType.IsActive == false)
                {
                    throw new NotFoundException("ClaimType not found");
                }

                existClaimType.IsActive = false;

                _repo.Update(existClaimType);
                return true;
            });
        }

    }
}
