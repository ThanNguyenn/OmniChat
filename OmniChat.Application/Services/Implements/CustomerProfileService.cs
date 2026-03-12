using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Requests.SupportConversation;
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
        private readonly IHubContext<SupportConversationHub> _hubContext;
        public CustomerProfileService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<CustomerProfileService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IHubContext<SupportConversationHub> hubContext) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _hubContext = hubContext;
        }

        public async Task<CustomerProfile> CreateCustomerProfileAsync(CreateCustomerProfileRequest request)
        {
            var existedProfile = await GetCustomerProfileBySenderAsync(
                request.ZaloSenderId
                ?? request.FacebookSenderId
                ?? request.InstagramSenderId
            );

            if (existedProfile != null)
                return existedProfile;

            var repo = _unitOfWork.GetRepository<CustomerProfile>();
            var entity = _mapper.Map<CustomerProfile>(request);

            await repo.InsertAsync(entity);
            await _unitOfWork.CommitAsync();

            return entity;
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

                return await repo.SingleOrDefaultAsync(predicate: cp =>
                cp.FacebookSenderId == senderId 
                || cp.ZaloSenderId == senderId ||
                cp.InstagramSenderId == senderId,
                include: cp => cp.Include(o => o.Orders)
                .Include(p => p.Invoices)
                );
        }

        public async Task<CustomerProfile> GetCustomerProfileByIdAsync(Guid customerProfileId)
        {
            var repo = _unitOfWork.GetRepository<CustomerProfile>();

            var existCustomerProfile = await repo.SingleOrDefaultAsync(
                predicate: x => x.Id == customerProfileId,
                 include: cp => cp.Include(o => o.Orders)
                .Include(p => p.Invoices)
                );

            if(existCustomerProfile == null)
                throw new NotFoundException("No CustomerProfile foundd");
            
            return existCustomerProfile;
        }

        public async Task<GetCustomerProfileResponse> GetCustomerProfileByEmailOrPhoneAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new BadRequestException("Email or Phone is required");

            var repo = _unitOfWork.GetRepository<CustomerProfile>();

            var existCustomProfile = await repo.SingleOrDefaultAsync(
                predicate: x => x.Email.Equals(keyword) || x.PhoneNumber.Equals(keyword),
                 include: cp => cp.Include(o => o.Orders)
                .Include(p => p.Invoices)
                );

            if (existCustomProfile == null)
                throw new NotFoundException("No CustomerProfile Found");

            var result = _mapper.Map<GetCustomerProfileResponse>(existCustomProfile); 
            return result;
        }

        public async Task<GetCustomerProfileResponse> GetCustomerProfileByCustomerIdAsync(Guid CustomerId)
        {
            if(CustomerId == Guid.Empty)
                throw new BadRequestException("CustomerId is required");

            var repo = _unitOfWork.GetRepository<CustomerProfile>();

            var existCustomProfile = await repo.SingleOrDefaultAsync(
               predicate: x => x.Id == CustomerId,
                include: cp => cp.Include(o => o.Orders)
               .Include(p => p.Invoices)
               );

            var result = _mapper.Map<GetCustomerProfileResponse>(existCustomProfile);
            return result;
        }

        public async Task<GetCustomerProfileResponse> UpdateCustomerProfileByIdAsync(Guid customerId,UpdateCustomerProfileRequest newInfor)
        {
            if (customerId == Guid.Empty)
                throw new BadRequestException("CustomerId is required");

            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var repo = _unitOfWork.GetRepository<CustomerProfile>();

                var customer = await repo.SingleOrDefaultAsync(predicate: x => x.Id == customerId);

                if (customer == null)
                    throw new NotFoundException("Customer not found");

            
                customer.CustomerName = newInfor.CustomerName ?? customer.CustomerName;
                customer.Address = newInfor.Address ?? customer.Address;
                customer.AvatarUrl = newInfor.AvatarUrl ?? customer.AvatarUrl;
                customer.Email = newInfor.Email ?? customer.Email;
                customer.PhoneNumber = newInfor.PhoneNumber ?? customer.PhoneNumber;
                customer.IsNewCustomer = false;

                 repo.Update(customer);

                var response = _mapper.Map<GetCustomerProfileResponse>(customer);

                
                await _hubContext.Clients.All.SendAsync(
                    "CustomerProfileUpdated",
                    response
                );

                return response;
            });
        }
    }
}
