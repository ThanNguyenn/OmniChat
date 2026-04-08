using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Application.Webhooks.Zalo.WebhookMessage;
using OmniChat.Infrastructure.Dtos.Requests.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Requests.SupportConversation;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Implements;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Resolver
{
    public class ZaloResolver : BaseService<ZaloResolver>
    {
        private readonly IProviderService _providerService;

        private readonly ICustomerProfileService _customerProfileService;

        private readonly ICustomerMergeService _customerMergeService;

        private readonly ICustomerMessageService _customerMessageService;

        private readonly IZaloUserService _zaloUserService;

        private readonly ISupportConversationService _supportConversationService;

        private readonly IMessageKeywordFilterService _messageKeywordFilterService;

        private readonly IChatAggregationService _chatAggregationService;

        private readonly IHubContext<SupportConversationHub> _hubContext;

        public ZaloResolver(
            IUnitOfWork<OmniChatDbContext> unitOfWork,
            ILogger<ZaloResolver> logger, IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IProviderService providerService,
            ICustomerProfileService customerProfileService,
            ICustomerMessageService customerMessageService,
            ICustomerMergeService customerMergeService,
            IZaloUserService zaloUserService,
            IFacebookUserService facebookUserService,
            ISupportConversationService supportConversationService,
            IConfiguration configuration,
            IMessageKeywordFilterService messageKeywordFilterService,
            IChatAggregationService chatAggregationService,
             IHubContext<SupportConversationHub> hubContext
            ) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _providerService = providerService;
            _customerProfileService = customerProfileService;
            _customerMessageService = customerMessageService;
            _customerMergeService = customerMergeService;
            _supportConversationService = supportConversationService;
            _zaloUserService = zaloUserService;
            _supportConversationService = supportConversationService;
            _chatAggregationService = chatAggregationService;
            _hubContext = hubContext;
            _messageKeywordFilterService = messageKeywordFilterService;
        }
        public async Task ZaloWebhookLogic(ZaloWebhookEvent zaloEvent)
        {
            SupportConversation conversation;

            _logger.LogInformation(
                "[ZALO] Webhook received | EventName={EventName} | SenderId={SenderId} | Timestamp={Timestamp}",
                zaloEvent?.EventName,
                zaloEvent?.Sender?.Id,
                zaloEvent?.Timestamp
            );

            if (zaloEvent == null)
            {
                _logger.LogWarning("[ZALO] Payload is NULL");
                return;
            }

            if (zaloEvent.Sender == null)
            {
                _logger.LogWarning("[ZALO] Sender is NULL");
                return;
            }

            if (zaloEvent.EventName != "user_send_text")
            {
                _logger.LogInformation(
                    "[ZALO] Ignored event | EventName={EventName}",
                    zaloEvent.EventName
                );
                return;
            }

            // Get Provider
            var provider = await _providerService.GetProviderByNameAsync("Zalo");
            if (provider == null)
            {
                _logger.LogError("[ZALO] Provider Zalo not found");
                throw new BusinessException("Provider Zalo not found");
            }

            _logger.LogInformation(
                "[ZALO] Provider loaded | ProviderId={ProviderId}",
                provider.Id
            );

            // Get CustomerProfile
            var customerProfile = await _customerProfileService.GetCustomerProfileBySenderAsync(
                zaloEvent.Sender.Id.ToString()
            );

            if (customerProfile == null)
            {
                _logger.LogInformation(
                    "[ZALO] CustomerProfile not found | SenderId={SenderId} → Creating new",
                    zaloEvent.Sender.Id
                );

                var zaloProfile = await _zaloUserService.GetUserProfileAsync(zaloEvent.Sender.Id);

                customerProfile = await _customerProfileService.CreateCustomerProfileAsync(
                    new CreateCustomerProfileRequest
                    {
                        ZaloSenderId = zaloEvent.Sender.Id.ToString(),
                        CustomerName = zaloProfile?.DisplayName ?? $"Zalo User {zaloEvent.Sender.Id}",
                        AvatarUrl = zaloProfile?.Avatar,
                        PhoneNumber = zaloProfile?.SharedInfo?.Phone,
                    }
                );

                _logger.LogInformation(
                    "[ZALO] CustomerProfile created | CustomerId={CustomerId}",
                    customerProfile.Id
                );
            }
            else
            {
                _logger.LogInformation(
                    "[ZALO] CustomerProfile found | CustomerId={CustomerId}",
                    customerProfile.Id
                );
            }

            // Find pending conversation
            var existConversation = await _supportConversationService
                .GetSupportConversationHavePendingByCustomerIdAsync(customerProfile.Id, provider.Id);

            if (existConversation != null)
            {
                conversation = existConversation;
            }
            else
            {
                var newSupportConversation = new CreateSupportConversationRequest
                {
                    ActiveCustomerId = customerProfile.Id,
                    ActiveStaffId = null,
                    AvatarUrl = customerProfile.AvatarUrl,
                    CustomerName = customerProfile.CustomerName,
                    IsDistributed = false,
                    ProvidersId = provider.Id,
                    Status = ConversationStatus.Pending,
                };

                try
                {
                    var newConversation = await _supportConversationService
                        .CreateNewSupportConversationAsync(newSupportConversation);

                    conversation = newConversation;
                }
                catch (DbUpdateException)
                {
                    var checkExistConversation = await _supportConversationService
                        .GetSupportConversationHavePendingByCustomerIdAsync(customerProfile.Id, provider.Id);

                    if (checkExistConversation == null)
                        throw;

                    conversation = checkExistConversation;
                }
            }

            var newMessage = await _customerMessageService.CreateCustomerMessageAsync(
                new CreateCustomerMessageRequest
                {
                    Content = zaloEvent.Message?.Text,
                    Timestamp = zaloEvent.Timestamp,
                    KeywordActive = false,
                    CustomerId = customerProfile.Id,
                    ConversationId = conversation.Id,
                }
            );

            // add to redis to sum message into string ,backgroud call AI service , assign
            await _chatAggregationService.AddMessageRedisAsync(customerProfile.Id, newMessage.Content,provider.Id);

            //if (conversation.ActiveStaffId == null)
            //{
            //    Guid staffId = Guid.Parse("89ceebe8-4ee8-4bf0-8893-977978dbc9e6");

            //    conversation = await _supportConversationService
            //        .AsignForSupportConversationByIdAsync(conversation, staffId);

            //    _logger.LogInformation(
            //        "[ZALO] Check Active Staff | ActiveStaff={ActiveStaffId}",
            //        conversation.ActiveStaffId
            //    );
            //}

            if (newMessage != null)
            {
                _logger.LogInformation(
                    "[ZALO] Message created | MessageId={MessageId}",
                    newMessage.Id
                );


                conversation.UpdateDate = DateTime.UtcNow;
                conversation.LastCustomerMessageAt = DateTime.UtcNow;
                conversation.ReminderSent = false;
                await _supportConversationService.UpdateConversationAsync(conversation);

                if (conversation.ActiveStaffId != null)
                {
                   

                    var unreadCount = await CountUnreadMessagesByConversationIdAsync(conversation.Id);

                    var sidebarUpdate = new StaffConversationSideBarUpdateResponse
                    {
                        ConversationId = conversation.Id,
                        CustomerName = conversation.CustomerName,
                        avartarUrl = conversation.AvatarUrl,
                        providerName = provider.ProviderName,
                        LastMessage = newMessage.Content,
                        UnreadMessageCount = unreadCount,
                    };

                    await _hubContext.Clients
                        .User(conversation.ActiveStaffId.ToString())
                        .SendAsync("SidebarUpdated", sidebarUpdate);

                    var extractResult = await _messageKeywordFilterService.ExtractKeywords(newMessage.Content);

                    var supportConversationMessages = new SupportConversationMessagesResponse
                    {
                        SenderType = "Customer",
                        SenderId = customerProfile.Id,
                        Content = newMessage.Content,
                        Timestamp = newMessage.Timestamp,
                        extractKeywordResponses =
                            (extractResult.Highlights.Count > 0 || extractResult.Recommends.Count > 0)
                                ? extractResult
                                : null
                    };

                    await _hubContext.Clients
                        .Group($"conversation:{conversation.Id}")
                        .SendAsync("CustomerReceiveMessage", supportConversationMessages);
                }
            }
            else
            {
                _logger.LogError(
                    "[ZALO] Failed to create message | SenderId={SenderId}",
                    customerProfile.ZaloSenderId
                );
            }
        }

        private async Task<int> CountUnreadMessagesByConversationIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<CustomerMessage>();

            return await repo.CountAsync(
                predicate: m => m.ConversationId == conversationId && m.IsRead == false
            );
        }
    }
}
