using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Services.Resolver;
using OmniChat.Application.SignalRHub;
using OmniChat.Application.Webhooks.Facebook.WebhookMessage;
using OmniChat.Application.Webhooks.Instagram.InstagramMessage;
using OmniChat.Application.Webhooks.Zalo.WebhookMessage;
using OmniChat.Infrastructure.Dtos.Requests.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Requests.Provider;
using OmniChat.Infrastructure.Dtos.Requests.SupportConversation;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class WebhookService : BaseService<WebhookService>, IWebhookService
    {
        private readonly IBackgroundTaskQueue _queue;

        private readonly ZaloResolver _zaloResolver;

        private readonly FacebookResolver _facebookResolver;

        private readonly IConfiguration _configuration;

        private readonly IServiceScopeFactory _scopeFactory;
        public WebhookService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<WebhookService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, ZaloResolver zaloResolver, FacebookResolver facebookResolver, IConfiguration configuration, IBackgroundTaskQueue queue, IServiceScopeFactory scopeFactory) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _zaloResolver = zaloResolver;
            _facebookResolver = facebookResolver;
            _configuration = configuration;
            _queue = queue;
            _scopeFactory = scopeFactory;
        }



        //========== Zalo //==========
        public async Task<bool> ZaloWebhookAsync(ZaloWebhookEvent zaloEvent)
        {
            await _queue.QueueAsync(async token =>
            {
                using var scope = _scopeFactory.CreateScope();

                var resolver = scope.ServiceProvider.GetRequiredService<ZaloResolver>();

                await resolver.ZaloWebhookLogic(zaloEvent);
            });

            return true;
        }

        //========== Facebook //==========

        public async Task<bool> FacebookWebhookAsync(FaceBookWebhookPayload faceBookWebhookPayload)
        {
            await _queue.QueueAsync(async token =>
            {
                using var scope = _scopeFactory.CreateScope();

                var resolver = scope.ServiceProvider.GetRequiredService<FacebookResolver>();

                await resolver.FacebookWebhookLogic(faceBookWebhookPayload);
            });

            return true;
        }

        public async Task<bool> VerifyFacebookWebhook(string mode, string token)
        {
            _logger.LogInformation(
                "[FACEBOOK][VERIFY] Verify webhook called | Mode={Mode} | Token={Token}",
                mode,
                string.IsNullOrEmpty(token) ? "NULL" : "REDACTED"
            );

            var verifyToken = _configuration["facebookWebHook:verifyToken"];

            if (string.IsNullOrEmpty(verifyToken))
            {
                _logger.LogError(
                    "[FACEBOOK][VERIFY] VerifyToken not found in configuration (facebookWebHook:verifyToken)"
                );
                return false;
            }

            if (mode != "subscribe")
            {
                _logger.LogWarning(
                    "[FACEBOOK][VERIFY] Invalid mode | Expected=subscribe | Actual={Mode}",
                    mode
                );
                return false;
            }

            if (token != verifyToken)
            {
                _logger.LogWarning(
                    "[FACEBOOK][VERIFY] Token mismatch | Provided={Provided} | Expected={Expected}",
                    "REDACTED",
                    "REDACTED"
                );
                return false;
            }

            _logger.LogInformation(
                "[FACEBOOK][VERIFY] Webhook verification SUCCESS"
            );

            return true;
        }


        //========== Instagram //==========

        //public async Task<bool> InstagramWebhookAsync(InstagramWebhookPayload payload)
        //{
        //    SupportConversation conversation;

        //    const string ourSenderId = "17841478357005004"; // if message is our past continue next message
        //    _logger.LogInformation("[INSTAGRAM] Webhook received");

        //    _logger.LogInformation(
        //        "[INSTAGRAM] RAW PAYLOAD:\n{Payload}",
        //        JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true })
        //    );

        //    if (payload?.entry == null || !payload.entry.Any())
        //    {
        //        _logger.LogWarning("[INSTAGRAM] Payload invalid or empty entry");
        //        return false;
        //    }

        //    var provider =
        //        await _providerService.GetProviderByNameAsync("Instagram")
        //        ?? throw new BusinessException("Provider Instagram Not found");

        //    bool result = false;

        //    foreach (var entry in payload.entry)
        //    {

        //        if (entry.messaging == null || !entry.messaging.Any())
        //        {
        //            _logger.LogWarning(
        //                "[INSTAGRAM] Entry has no messaging | BusinessId={BusinessId}",
        //                entry.id
        //            );
        //            continue;
        //        }

        //        foreach (var msg in entry.messaging)
        //        {
        //            if (msg.Sender.id == ourSenderId)
        //            {
        //                _logger.LogInformation(
        //                    "[INSTAGRAM] Ignored staff message | BusinessId={BusinessId} | SenderId={SenderId}",
        //                    entry.id,
        //                    msg.Sender?.id
        //                );
        //                continue;
        //            }

        //            if (msg.Message?.text == null)
        //            {
        //                _logger.LogInformation(
        //                    "[INSTAGRAM] Ignored non-text message | BusinessId={BusinessId} | SenderId={SenderId}",
        //                    entry.id,
        //                    msg.Sender?.id
        //                );
        //                continue;
        //            }

        //            var businessId = entry.id;      // Instagram Business
        //            var senderId = msg.Sender.id;  // Customer
        //            var text = msg.Message.text;

        //            _logger.LogInformation(
        //                "[INSTAGRAM] Message received | BusinessId={BusinessId} | SenderId={SenderId} | Text={Text}",
        //                businessId,
        //                senderId,
        //                text
        //            );

        //            // ==== BUSINESS LOGIC ====

        //            var customerProfile =
        //                await _customerProfileService.GetCustomerProfileBySenderAsync(
        //                    senderId: senderId
        //                );

        //            if (customerProfile == null)
        //            {
        //                _logger.LogInformation(
        //                    "[INSTAGRAM] CustomerProfile not found | SenderId={SenderId} → Creating new",
        //                    senderId
        //                );

        //                var igUser = await _instagramUserService.GetUserProfileAsync(senderId);

        //                customerProfile =
        //                    await _customerProfileService.CreateCustomerProfileAsync(
        //                        new CreateCustomerProfileRequest
        //                        {
        //                            InstagramSenderId = senderId,
        //                            CustomerName = igUser?.Name ?? "Instagram User",
        //                            AvatarUrl = igUser?.ProfilePictureUrl,
        //                        }
        //                    );
        //            }


        //            var existConversation = await _supportConversationService.GetSupportConversationHavePendingByCustomerIdAsync(customerProfile.Id, provider.Id);

        //            if (existConversation != null)
        //            {
        //                conversation = existConversation;

        //            }
        //            else
        //            {
        //                // if no have pending conversation
        //                var newSupportConversation = new CreateSupportConversationRequest
        //                {
        //                    ActiveCustomerId = customerProfile.Id,
        //                    ActiveStaffId = null,
        //                    AvatarUrl = customerProfile.AvatarUrl,
        //                    CustomerName = customerProfile.CustomerName,
        //                    IsDistributed = true,
        //                    ProvidersId = provider.Id,
        //                    Status = ConversationStatus.Pending,

        //                };
        //                try
        //                {
        //                    // dung cho truong hop 2 tin nhan gui toi cung luc 
        //                    var newComversation = await _supportConversationService.CreateNewSupportConversationAsync(newSupportConversation);

        //                    conversation = newComversation;
        //                }
        //                catch (DbUpdateException)
        //                {
        //                    var checkExistConversation =
        //                        await _supportConversationService
        //                            .GetSupportConversationHavePendingByCustomerIdAsync(
        //                                customerProfile.Id,
        //                                provider.Id);

        //                    if (checkExistConversation == null)
        //                        throw;

        //                    conversation = checkExistConversation;
        //                }


        //            }

        //            var newMessage =
        //                await _customerMessageService.CreateCustomerMessageAsync(
        //                    new CreateCustomerMessageRequest
        //                    {
        //                        Content = text,
        //                        Timestamp = msg.Timestamp,
        //                        KeywordActive = false,
        //                        CustomerId = customerProfile.Id,
        //                        ConversationId = conversation.Id,
        //                    }
        //                );


        //            if (conversation.ActiveStaffId == null)
        //            {

        //                // asign Staff after run distribute

        //                Guid StaffId = Guid.Parse("89ceebe8-4ee8-4bf0-8893-977978dbc9e6");

        //                conversation = await _supportConversationService.AsignForSupportConversationByIdAsync(conversation, StaffId);

        //                _logger.LogInformation(
        //                  "[Facebook] Check Actvice Staff | ActiveStaff={ActiveStaffId}",
        //                  conversation.ActiveStaffId);
        //            }


        //            if (newMessage != null)
        //            {
        //                _logger.LogInformation(
        //                    "[INSTAGRAM] Message created | MessageId={MessageId}",
        //                    newMessage.Id
        //                );
        //                result = true;

        //                // After get new customer message Update Supportconversation UpdateDate -> now
        //                var updatedConversation = await _supportConversationService.UpdateSupportConversationUpdateDateAsync(conversation);

        //                if (updatedConversation.ActiveStaffId != null)
        //                {
        //                    // Add SignalR Realtime for sidebar staff 

        //                    var unreadCount = await CountUnreadMessagesByConversationIdAsync(updatedConversation.Id);


        //                    var sidebarUpdate = new StaffConversationSideBarUpdateResponse
        //                    {
        //                        ConversationId = updatedConversation.Id,
        //                        CustomerName = updatedConversation.CustomerName,
        //                        avartarUrl = updatedConversation.AvatarUrl,
        //                        providerName = provider.ProviderName,
        //                        LastMessage = newMessage.Content,
        //                        UnreadMessage = unreadCount,
        //                    };
        //                    await _hubContext.Clients
        //                       .User(updatedConversation.ActiveStaffId.ToString())
        //                       .SendAsync("SidebarUpdated", sidebarUpdate);

        //                    // Add SignalR realTime for chat detail if staff is viewing

        //                    //Extract keyword + recommend
        //                    var extractResult = await _messageKeywordFilterService.ExtractKeywords(newMessage.Content);

        //                    var supportConversationMessages = new SupportConversationMessagesResponse
        //                    {
        //                        SenderType = "Customer",
        //                        SenderId = customerProfile.Id,
        //                        Content = newMessage.Content,
        //                        Timestamp = newMessage.Timestamp,
        //                        extractKeywordResponses =
        //                        (extractResult.Highlights.Count > 0 ||
        //                         extractResult.Recommends.Count > 0)
        //                            ? extractResult
        //                            : null
        //                    };

        //                    await _hubContext.Clients.Group($"conversation:{updatedConversation.Id}")
        //                   .SendAsync("CustomerReceiveMessage", supportConversationMessages);

        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}


        //public async Task<bool> VerifyInstagramWebhook(string mode, string token)
        //{
        //    _logger.LogInformation(
        //        "[INSTAGRAM][VERIFY] Verify webhook called | Mode={Mode} | Token={Token}",
        //        mode,
        //        string.IsNullOrEmpty(token) ? "NULL" : "REDACTED"
        //    );

        //    var verifyToken = _configuration["InstagramWebhook:verifyToken"];

        //    if (string.IsNullOrEmpty(verifyToken))
        //    {
        //        _logger.LogError(
        //            "[INSTAGRAM][VERIFY] VerifyToken not found in configuration (InstagramWebhook:verifyToken)"
        //        );
        //        return false;
        //    }

        //    if (mode != "subscribe")
        //    {
        //        _logger.LogWarning(
        //            "[INSTAGRAM][VERIFY] Invalid mode | Expected=subscribe | Actual={Mode}",
        //            mode
        //        );
        //        return false;
        //    }

        //    if (token != verifyToken)
        //    {
        //        _logger.LogWarning(
        //            "[INSTAGRAM][VERIFY] Token mismatch | Provided={Provided} | Expected={Expected}",
        //            "REDACTED",
        //            "REDACTED"
        //        );
        //        return false;
        //    }

        //    _logger.LogInformation(
        //        "[INSTAGRAM][VERIFY] Webhook verification SUCCESS"
        //    );

        //    return true;
        //}
    }
}
