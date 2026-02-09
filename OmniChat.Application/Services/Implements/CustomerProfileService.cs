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


        public async Task<CustomerProfile> CreateCustomerProfileAsync(CreateCustomerProfileRequest createCustomerProfileRequest)
        {
           
                return await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var repo = _unitOfWork.GetRepository<CustomerProfile>();

                    var existedProfile = await repo.SingleOrDefaultAsync(
                        predicate: x =>
                            x.FacebookSenderId == createCustomerProfileRequest.FacebookSenderId &&
                            x.InstagramSenderId == createCustomerProfileRequest.InstagramSenderId &&
                            x.ZaloSenderId == createCustomerProfileRequest.ZaloSenderId
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
                        AvatarUrl = x.AvatarUrl,
                        FacebookSenderId = x.FacebookSenderId,
                        ZaloSenderId = x.ZaloSenderId,
                        InstagramSenderId = x.InstagramSenderId,
                    },
                    predicate: string.IsNullOrWhiteSpace(customerName)
                        ? null
                        : x => x.CustomerName.Contains(customerName),
                    orderBy: q => q.OrderByDescending(x => x.CustomerName),
                    page: pageNumber,
                    size: pageSize
                ); 
        }

        public async Task<CustomerProfile> GetCustomerProfileBySenderAsync(string senderId)
        {
            
                var repo = _unitOfWork.GetRepository<CustomerProfile>();

                return await repo.SingleOrDefaultAsync(predicate: x => 
                x.FacebookSenderId == senderId 
                || x.ZaloSenderId == senderId ||
                x.InstagramSenderId == senderId);
        }

        public async Task<CustomerProfile> GetCustomerProfileByIdAsync(Guid customerProfileId)
        {
            var repo = _unitOfWork.GetRepository<CustomerProfile>();

            var existCustomerProfile = await repo.SingleOrDefaultAsync(predicate: x => x.Id == customerProfileId);

            if(existCustomerProfile == null)
                throw new NotFoundException("No CustomerProfile foundd");
            
            return existCustomerProfile;
        }

        public async Task<GetCustomerProfileResponse> GetCustomerProfileByEmailOrPhoneAsync(string email, string phone)
        {
            var repo = _unitOfWork.GetRepository<CustomerProfile>();

            var existCustomProfile = await repo.SingleOrDefaultAsync(
                predicate: x => x.Email.Equals(email) || x.PhoneNumber.Equals(phone));

            if (existCustomProfile == null)
                throw new NotFoundException("No CustomerProfile Found");

            var result = _mapper.Map<GetCustomerProfileResponse>(existCustomProfile); 
            return result;
        }




        public async Task<GetCustomerProfileResponse> MergeAndDeleteAsync(Guid sourceId, Guid targetId)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var repo = _unitOfWork.GetRepository<CustomerProfile>();

                var source = await GetCustomerProfileByIdAsync(sourceId);
                var target = await GetCustomerProfileByIdAsync(targetId);

                if (source == null || target == null)
                    throw new BusinessException("Customer not found");

                if (target.Id == source.Id)
                    throw new BusinessException("Cannot merge same customer");

                target.FacebookSenderId ??= source.FacebookSenderId;
                target.ZaloSenderId ??= source.ZaloSenderId;
                target.InstagramSenderId ??= source.InstagramSenderId;

                await _customerMessageRepo.UpdateCustomerIdAsync(
                   source.Id,
                   target.Id
                );

                await _supportConversationRepo.UpdateCustomerIdAsync(
                  source.Id,
                  target.Id
                 );

                await repo.DeleteAsync(source);
            });
        }

    }
}
