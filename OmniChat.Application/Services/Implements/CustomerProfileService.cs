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
        private readonly IWalletService _walletService;

        public CustomerProfileService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<CustomerProfileService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IHubContext<SupportConversationHub> hubContext, IWalletService walletService ) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _hubContext = hubContext;
            _walletService = walletService;
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
            var searchTerm = customerName?.Trim().ToUpper();

            var pagingData = await repo.GetPagingListAsync(
                selector: x => new GetCustomerProfileResponse
                {
                    Id = x.Id,
                    CustomerName = x.CustomerName,
                    AvatarUrl = x.AvatarUrl,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    FacebookSenderId = x.FacebookSenderId,
                    ZaloSenderId = x.ZaloSenderId,
                    InstagramSenderId = x.InstagramSenderId,
                    CustomerDate = x.CreateDate,
                    TotalOrder = x.Orders.Count,

                    TotalPayment =
    (x.Wallet.Transactions
        .Where(t => t.TransactionType == TransactionType.Deposit)
        .Sum(t => t.Amount)
    -
    x.Wallet.Transactions
        .Where(t => t.TransactionType == TransactionType.Refund)
        .Sum(t => t.Amount))
                },
                predicate: string.IsNullOrWhiteSpace(searchTerm)
                    ? null
                    : x => x.CustomerName.ToUpper().Contains(searchTerm),
                orderBy: q => q.OrderByDescending(x => x.CustomerName),
                include: cp => cp.Include(o => o.Orders).Include(p => p.Invoices).Include(s => s.Wallet).ThenInclude(cc => cc.Transactions),
                page: pageNumber,
                size: pageSize
            );

            _unitOfWork.Context.ChangeTracker.Clear();
            foreach (var item in pagingData.Items)
            {
                item.getWalletResponse = await _walletService.CalculateWallet(item.Id);
            }
            return pagingData;
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
                throw new NotFoundException("Không tìm thấy hồ sơ khách hàng với ID: {customerProfileId}");
            
            return existCustomerProfile;
        }

        public async Task<GetCustomerProfileResponse> GetCustomerProfileByEmailOrPhoneAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new BadRequestException("Vui lòng cung cấp Email hoặc Số điện thoại để tìm kiếm.");

            var searchRequest = keyword.Trim();

            var repo = _unitOfWork.GetRepository<CustomerProfile>();

            var existCustomProfile = await repo.SingleOrDefaultAsync(
                 predicate: x => x.Email == searchRequest || x.PhoneNumber == searchRequest,
                 include: cp => cp.Include(o => o.Orders)
                                  .Include(p => p.Invoices)
             );

            if (existCustomProfile == null)
                throw new NotFoundException("Không tìm thấy khách hàng với thông tin đã cung cấp.");

            var result = _mapper.Map<GetCustomerProfileResponse>(existCustomProfile);

            if (result != null)
            {
                result.getWalletResponse = await _walletService.CalculateWallet(existCustomProfile.Id);
            }   

            return result;
        }

        public async Task<GetCustomerProfileResponse> GetCustomerProfileByCustomerIdAsync(Guid CustomerId)
        {
            if(CustomerId == Guid.Empty)
                throw new BadRequestException("Mã khách hàng (CustomerId) không được để trống.");

            var repo = _unitOfWork.GetRepository<CustomerProfile>();

            var existCustomProfile = await repo.SingleOrDefaultAsync(
               predicate: x => x.Id == CustomerId,
                include: cp => cp.Include(o => o.Orders)
               .Include(p => p.Invoices)
               );

            if (existCustomProfile == null)
                throw new NotFoundException("Hồ sơ khách hàng không tồn tại.");

            var result = _mapper.Map<GetCustomerProfileResponse>(existCustomProfile);

            if (result != null)
            {
                result.getWalletResponse = await _walletService.CalculateWallet(CustomerId);
            }

            return result;
        }

        public async Task<GetCustomerProfileResponse> UpdateCustomerProfileByIdAsync(Guid customerId,UpdateCustomerProfileRequest newInfor)
        {
            if (customerId == Guid.Empty)
                throw new BadRequestException("Mã khách hàng (CustomerId) không hợp lệ.");

            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var repo = _unitOfWork.GetRepository<CustomerProfile>();

                var customer = await repo.SingleOrDefaultAsync(predicate: x => x.Id == customerId);

                if (customer == null)
                    throw new NotFoundException("Không tìm thấy khách hàng để cập nhật.");

            
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

        public async Task<CustomerDetailResponse> GetCustomerDetailByConversationIdAsync(Guid conversationId)
        {
            if (conversationId == Guid.Empty)
                throw new BadRequestException("Mã hội thoại (ConversationId) là bắt buộc.");

            var supportConversation = await _unitOfWork.GetRepository<SupportConversation>().SingleOrDefaultAsync(
                predicate: x => x.Id == conversationId,
                include: sc => sc.Include(sc => sc.Providers)
            );

            if (supportConversation == null) throw new NotFoundException("Cuộc hội thoại hỗ trợ không tồn tại.");

            var customer = await _unitOfWork.GetRepository<CustomerProfile>().SingleOrDefaultAsync(
                predicate: x => x.Id == supportConversation.ActiveCustomerId,
                include: cp => cp.Include(o => o.Orders).Include(x => x.Invoices)
            );

            if (customer == null) throw new NotFoundException("Không tìm thấy thông tin khách hàng liên quan đến hội thoại này.");

            var provider = await _unitOfWork.GetRepository<Provider>().SingleOrDefaultAsync(
                predicate: p => p.Id == supportConversation.ProvidersId
            );

            var result = new CustomerDetailResponse
            {
                Id = customer.Id,
                AvatarUrl = customer.AvatarUrl,
                CustomerName = customer.CustomerName,
                CustomerPhone = customer.PhoneNumber,
                Email = customer.Email,
                Address = customer.Address,
                BecomeCustomerDate = customer.CreateDate,
                TotalOrder = customer.Orders?.Count ?? 0,
                TotalPay = customer.Invoices?.Sum(p => (double)(p.Total - (p.DeductedAmount))) ?? 0,

               
                ProviderName = provider?.ProviderName,
                TimeStartSupport = supportConversation.CreatedDate,

                
                getWalletResponse = await _walletService.CalculateWallet(customer.Id)
            };

            return result;

        }

        public async Task UpdateIsformSentCustomerProfileAsync(Guid customerProfileId)
        {
            if (customerProfileId == Guid.Empty)
                throw new BadRequestException("Mã hồ sơ khách hàng không hợp lệ.");

            var repo = _unitOfWork.GetRepository<CustomerProfile>();

            var customerProfile = await repo.GetByIdAsync(customerProfileId);

            if (customerProfile == null)
                throw new NotFoundException("Hồ sơ khách hàng không tồn tại để cập nhật trạng thái Form.");

            customerProfile.IsFormSent = true;

                repo.Update(customerProfile);

                await _unitOfWork.CommitAsync();
        }
    }
}
