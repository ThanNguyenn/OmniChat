using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Application.Webhooks.Facebook.WebhookMessage;
using OmniChat.Infrastructure.Dtos.Requests.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Requests.SupportConversation;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Resolver
{
    public class FacebookResolver : BaseService<FacebookResolver>
    {
        private readonly IProviderService _providerService;

        private readonly ICustomerProfileService _customerProfileService;

        private readonly ICustomerMessageService _customerMessageService;

        private readonly ICustomerMergeService _customerMergeService;

        private readonly IFacebookUserService _facebookUserService;

        private readonly ISupportConversationService _supportConversationService;

        private readonly IMessageKeywordFilterService _messageKeywordFilterService;

        private readonly IChatAggregationService _chatAggregationService;

        private readonly IHubContext<SupportConversationHub> _hubContext;

        public FacebookResolver(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<FacebookResolver> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IProviderService providerService, ICustomerProfileService customerProfileService, ICustomerMessageService customerMessageService, IZaloUserService zaloUserService, IFacebookUserService facebookUserService, IConfiguration configuration, IInstagramUserService instagramUserService, IHubContext<SupportConversationHub> hubContext, ISupportConversationService supportConversationService, IMessageKeywordFilterService messageKeywordFilterService, IChatAggregationService chatAggregationService, ICustomerMergeService customerMergeService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _providerService = providerService;
            _customerProfileService = customerProfileService;
            _customerMessageService = customerMessageService;
            _supportConversationService = supportConversationService;
            _messageKeywordFilterService = messageKeywordFilterService;
            _facebookUserService = facebookUserService;
            _hubContext = hubContext;
            _supportConversationService = supportConversationService;
            _chatAggregationService = chatAggregationService;
            _customerMergeService = customerMergeService;
        }

        public async Task FacebookWebhookLogic(FaceBookWebhookPayload faceBookWebhookPayload)
        {
            SupportConversation conversation;

            _logger.LogInformation("[FACEBOOK] Webhook received");

            if (faceBookWebhookPayload?.entry == null || !faceBookWebhookPayload.entry.Any())
            {
                _logger.LogWarning("[FACEBOOK] Payload invalid or empty entry");
                return;
            }

            var provider = await _providerService.GetProviderByNameAsync("Facebook")
                ?? throw new BusinessException("Provider Facebook Not found");

            _logger.LogInformation(
                "[FACEBOOK] Provider loaded | ProviderId={ProviderId}",
                provider.Id
            );

            foreach (var entry in faceBookWebhookPayload.entry)
            {
                if (entry.messaging == null || !entry.messaging.Any())
                {
                    _logger.LogWarning("[FACEBOOK] Entry has no messages");
                    continue;
                }

                foreach (var messaging in entry.messaging)
                {
                    if (messaging.sender?.id == null)
                    {
                        _logger.LogWarning("[FACEBOOK] SenderId is NULL");
                        continue;
                    }

                    if (messaging.message?.text == null)
                    {
                        _logger.LogInformation(
                            "[FACEBOOK] Ignored non-text message | SenderId={SenderId}",
                            messaging.sender.id
                        );
                        continue;
                    }

                    _logger.LogInformation(
                        "[FACEBOOK] Message received | SenderId={SenderId} | Text={Text}",
                        messaging.sender.id,
                        messaging.message.text
                    );

                    var customerProfile = await _customerProfileService.GetCustomerProfileBySenderAsync(
                        messaging.sender.id
                    );

                    if (customerProfile == null)
                    {
                        _logger.LogInformation(
                            "[FACEBOOK] CustomerProfile not found | SenderId={SenderId} → Creating new",
                            messaging.sender.id
                        );

                        var fbUser = await _facebookUserService.GetUserProfileAsync(messaging.sender.id);

                        customerProfile = await _customerProfileService.CreateCustomerProfileAsync(
                            new CreateCustomerProfileRequest
                            {
                                FacebookSenderId = messaging.sender.id,
                                CustomerName = $"{fbUser?.FirstName} {fbUser?.LastName}".Trim(),
                                AvatarUrl = fbUser?.ProfilePic,
                            }
                        );

                        _logger.LogInformation(
                            "[FACEBOOK] CustomerProfile created | CustomerId={CustomerId}",
                            customerProfile.Id
                        );
                    }

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
                            Content = messaging.message.text,
                            Timestamp = messaging.timestamp,
                            KeywordActive = false,
                            CustomerId = customerProfile.Id,
                            ConversationId = conversation.Id,
                        }
                    );

                    // add to redis to sum message into string ,backgroud call AI service
                    await _chatAggregationService.AddMessageRedisAsync(customerProfile.Id, newMessage.Content, provider.Id);

                    //if (conversation.ActiveStaffId == null)
                    //{
                    //    Guid staffId = Guid.Parse("89ceebe8-4ee8-4bf0-8893-977978dbc9e6");

                    //    conversation = await _supportConversationService
                    //        .AsignForSupportConversationByIdAsync(conversation, staffId);

                    //    _logger.LogInformation(
                    //        "[FACEBOOK] Check Active Staff | ActiveStaff={ActiveStaffId}",
                    //        conversation.ActiveStaffId
                    //    );
                    //}

                    if (newMessage != null)
                    {
                        _logger.LogInformation(
                            "[FACEBOOK] Message created | MessageId={MessageId}",
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
                            "[FACEBOOK] Failed to create message | SenderId={SenderId}",
                            messaging.sender.id
                        );
                    }
                }
            }
        }

        private async Task<int> CountUnreadMessagesByConversationIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<CustomerMessage>();

            return await repo.CountAsync(predicate: m => m.ConversationId == conversationId && m.IsRead == false);
        }
    }
}
