using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
using OmniChat.Infrastructure.Exceptions;
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
    public class CustomerMergeService : BaseService<CustomerMergeService>, ICustomerMergeService
    {
        private readonly ICustomerProfileService _customerProfileService;
        private readonly ICustomerMessageService _customerMessageService;
        private readonly ISupportConversationService _supportConversationService;
        private readonly ISupportStaffMessageService _supportStaffMessageService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHubContext<SupportConversationHub> _hubContext;
        private readonly IWalletService _walletService;

        public CustomerMergeService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<CustomerMergeService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor,
             ICustomerProfileService customerProfileService,
            ICustomerMessageService customerMessageService,
            ISupportConversationService supportConversationService,
            IHubContext<SupportConversationHub> hubContext,
            ISupportStaffMessageService supportStaffMessageService,
            IServiceScopeFactory serviceScopeFactory,
            IWalletService walletService
            ) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _customerProfileService = customerProfileService;
            _customerMessageService = customerMessageService;
            _supportConversationService = supportConversationService;
            _supportStaffMessageService = supportStaffMessageService;
            _serviceScopeFactory = serviceScopeFactory;
            _hubContext = hubContext;
            _walletService = walletService;
        }

        public async Task<GetCustomerProfileResponse> MergeAndDeleteAsync(Guid sourceId, Guid targetId)
        {
            _logger.LogInformation("Start MergeAndDeleteAsync | SourceId: {SourceId}, TargetId: {TargetId}", sourceId, targetId);

            var customerRepo = _unitOfWork.GetRepository<CustomerProfile>();

            var source = await _customerProfileService.GetCustomerProfileByIdAsync(sourceId);
            var target = await _customerProfileService.GetCustomerProfileByIdAsync(targetId);

            if (source.Id == target.Id)
                throw new BusinessException("Cannot merge same customer");

            _logger.LogInformation("Merging profiles | Source: {SourceId} -> Target: {TargetId}", source.Id, target.Id);


            target.FacebookSenderId ??= source.FacebookSenderId;
            target.ZaloSenderId ??= source.ZaloSenderId;
            target.InstagramSenderId ??= source.InstagramSenderId;


            target.CustomerName = MergeField(target.CustomerName, source.CustomerName);
            target.Email = MergeField(target.Email, source.Email);
            target.PhoneNumber = MergeField(target.PhoneNumber, source.PhoneNumber);
            target.Address = MergeField(target.Address, source.Address);
            target.AvatarUrl = MergeField(target.AvatarUrl, source.AvatarUrl);
            target.IsNewCustomer = false;


            await _customerMessageService.UpdateCustomerMessageAfterMergeAsync(source, target);
            await _supportConversationService.UpdateConversationAfterMergeAsync(source, target);

            customerRepo.Update(target);


            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Committed FK re-assignments and target update");

            await customerRepo.DeleteAsync(x => x.Id == source.Id);
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Deleted source profile {SourceId}", source.Id);

            var response = _mapper.Map<GetCustomerProfileResponse>(target);

            await _hubContext.Clients.All.SendAsync("SidebarCustomerUpdated", response);

            return response;
        }

        private string? MergeField(string? targetValue, string? sourceValue)
        {
            if (string.IsNullOrWhiteSpace(targetValue))
                return sourceValue;

            return targetValue;
        }

        public async Task HandleEnrichCustomerAsync(EnrichCustomerRequest dto)
        {

            _logger.LogInformation("Start EnrichCustomer | ProfileId: {ProfileId}", dto.ProfileId);

            var repo = _unitOfWork.GetRepository<CustomerProfile>();

            var current = await _customerProfileService.GetCustomerProfileByIdAsync(dto.ProfileId);

            if (current == null)
            {
                _logger.LogError("Profile not found: {ProfileId}", dto.ProfileId);
                throw new NotFoundException("Profile not found");
            }



            var email = dto.Email?.Trim().ToLower();
            var phone = NormalizePhone(dto.Phone);

            _logger.LogInformation("Normalized data | Email: {Email}, Phone: {Phone}", email, phone);

            var existing = await repo.SingleOrDefaultAsync(predicate: x =>
                (email != null && x.Email == email) ||
                (phone != null && x.PhoneNumber == phone)
            );


            if (existing != null && existing.Id != current.Id)
            {
                _logger.LogInformation("Found duplicate profile {ExistingId}, merging...", existing.Id);
                await MergeAndDeleteAsync(current.Id, existing.Id);
                return;
            }

            if (current.IsProfileCompleted)
            {
                _logger.LogInformation("Profile {ProfileId} already completed, skipping", current.Id);
                return;
            }

            current.Email = email;
            current.PhoneNumber = phone;
            current.Address = dto.Address;
            current.IsNewCustomer = false;
            current.IsProfileCompleted = true;

            await _walletService.CreateWallet(current.Id);

            repo.Update(current);

            await _unitOfWork.CommitAsync();
            _logger.LogInformation("Updated profile successfully {ProfileId}", current.Id);
        }

        private string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;

            phone = phone.Trim()
                         .Replace(" ", "")
                         .Replace(".", "")
                         .Replace("-", "");

            if (phone.StartsWith("+84"))
                phone = "0" + phone.Substring(3);

            return phone;
        }

        public async Task SendFormLinkIfNeededAsync(SupportConversation conversation)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var scopedCustomerService = scope.ServiceProvider.GetRequiredService<ICustomerProfileService>();
            var scopedMessageService = scope.ServiceProvider.GetRequiredService<ISupportStaffMessageService>();

            try
            {
                if (conversation.ActiveCustomerId == null) return;

                var customer = await scopedCustomerService
                    .GetCustomerProfileByIdAsync(conversation.ActiveCustomerId);

                if (customer == null || customer.IsFormSent) return;

                var ZALO_ID = Guid.Parse("bb4a4a44-4b03-442f-9a5e-a43ad45391a0");
                var FB_ID = Guid.Parse("67c4f1fd-9612-4a22-a30d-809b1598455b");

                var message = $"Chào bạn, vui lòng bổ sung thông tin tại đây để chúng tôi hỗ trợ tốt nhất: " +
                              $"https://customer-form-black.vercel.app/?profileId={customer.Id}";

                var request = new CreateSupportStaffMessageRequest
                {
                    SupportConversationId = conversation.Id,
                    StaffId = conversation.ActiveStaffId ?? Guid.Empty,
                    Content = message
                };

                bool canUpdateDb = false;
                try
                {
                    if (conversation.ProvidersId == ZALO_ID)
                        await scopedMessageService.SendZaloMessageAsync(request);
                    else if (conversation.ProvidersId == FB_ID)
                        await scopedMessageService.SendFacebookMesageAsync(request);
                    else
                    {
                        _logger.LogWarning("Unknown Provider ID: {Id}", conversation.ProvidersId);
                        return;
                    }

                    canUpdateDb = true;
                }
                catch (Exception apiEx)
                {
                    _logger.LogError("Provider API Failed: {Msg}", apiEx.Message);
                    canUpdateDb = false;
                }

                if (canUpdateDb)
                    await scopedCustomerService.UpdateIsformSentCustomerProfileAsync(customer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FATAL ERROR in SendFormLinkIfNeededAsync for Customer {Id}",
                    conversation.ActiveCustomerId);
                throw;
            }
        }
    }
}