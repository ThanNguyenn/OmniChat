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


        public async Task<CustomerProfile> CreateCustomerProfileEntityAsync(CreateCustomerProfileRequest createCustomerProfileRequest)
        {
           
                return await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var repo = _unitOfWork.GetRepository<CustomerProfile>();

                    var existedProfile = await repo.SingleOrDefaultAsync(
                        predicate: x =>
                            x.SenderId == createCustomerProfileRequest.SenderId &&
                            x.ProvidersId == createCustomerProfileRequest.ProvidersId
                    );

                    if (existedProfile != null)
                        return existedProfile;

                    var entity = _mapper.Map<CustomerProfile>(createCustomerProfileRequest);

                    await repo.InsertAsync(entity);

                    return entity;
                });
           
        }

        public async Task<PagingResponse<GetCustomerProfileResponse>> GetCustomerProfilesPagingAsync(int pageNumber = 1, int pageSize = 20, string? customerName = null)
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

        public async Task<CustomerProfile> GetCustomerProfileBySenderAndProviderIdIdAsync(string senderId, Guid providersId)
        {
            
                var repo = _unitOfWork.GetRepository<CustomerProfile>();

                return await repo.SingleOrDefaultAsync(predicate: x => x.SenderId == senderId && x.ProvidersId == providersId);
        }

        public async Task<CustomerProfile> GetCustomerProfileByIdAsync(Guid customerProfileId)
        {
            var repo = _unitOfWork.GetRepository<CustomerProfile>();

            var existCustomerProfile = await repo.SingleOrDefaultAsync(predicate: x => x.Id == customerProfileId);

            if(existCustomerProfile == null)
                throw new NotFoundException("No CustomerProfile foundd");
            
            return existCustomerProfile;
        }
    }
}
