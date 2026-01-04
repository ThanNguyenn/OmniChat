using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
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
    public class CustomerProfileService : BaseService<CustomerProfileService>, ICustomerProfileService
    {
        public CustomerProfileService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<CustomerProfileService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }


        public async Task<CreateCustomerProfileResponse> CreateNewCustomerProfileAsync(CreateCustomerProfileRequest createCustomerProfileRequest)
        {
            try
            {
                return await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var repo = _unitOfWork.GetRepository<CustomerProfile>();

                    // check duplicate theo SenderId + Provider
                    var existedProfile = await repo.SingleOrDefaultAsync(
                         selector: x => x,
                         predicate: x =>
                             x.SenderId == createCustomerProfileRequest.SenderId &&
                             x.ProvidersId == createCustomerProfileRequest.ProvidersId
                     );

                    if (existedProfile != null)
                        throw new BusinessException(
                            "Customer profile already exists for this provider.");

                    // Map request -> entity
                    var entity = _mapper.Map<CustomerProfile>(createCustomerProfileRequest);

                    //Add entity
                    await repo.InsertAsync(entity);

                    //Map entity -> response
                    return _mapper.Map<CreateCustomerProfileResponse>(entity);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating customer profile: {Message}",
                    ex.Message);
                throw;
            }
        }

        public async Task<PagingResponse<GetCustomerProfileResponse>> GetCustomerProfilesPagingAsync(int pageNumber = 1, int pageSize = 20, string? customerName = null)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<CustomerProfile>();

                return await repo.GetPagingListAsync(
                    selector: x => new GetCustomerProfileResponse
                    {
                        Id = x.Id,
                        CustomerName = x.CustomerName,
                        ProvidersId = x.ProvidersId,
                        Email = x.Email,
                        PhoneNumber = x.PhoneNumber,
                        Gender = x.Gender,
                        DateOfBirth = x.DateOfBirth,
                        AvatarUrl = x.AvatarUrl,
                        SenderId = x.SenderId
                    },
                    predicate: string.IsNullOrWhiteSpace(customerName)
                        ? null
                        : x => x.CustomerName.Contains(customerName),
                    orderBy: q => q.OrderByDescending(x => x.CustomerName),
                    page: pageNumber,
                    size: pageSize
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error paging CustomerProfile: {Message}",
                    ex.Message);
                throw;
            }
        }

        public async Task<CustomerProfile> GetCustomerProfileBySenderIdAsync(long senderId)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<CustomerProfile>();

                return await repo.SingleOrDefaultAsync(predicate: x => x.SenderId == senderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                   ex,
                   "Error Get CustomerProfile By SenderId: {Message}",
                   ex.Message);
                throw;
            }
        }
    }
}
