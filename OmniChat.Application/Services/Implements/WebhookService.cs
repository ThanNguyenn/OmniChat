using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using OmniChat.Application.Services.Interface;
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
        private readonly IProviderService _providerService;

        private readonly ICustomerProfileService _customerProfileService;

        private readonly ICustomerMessageService _customerMessageService;

        private readonly IZaloUserService _zaloUserService;

        private readonly IFacebookUserService _facebookUserService;

        private readonly IInstagramUserService _instagramUserService;

        private readonly ISupportConversationService _supportConversationService;

        private readonly IConfiguration _configuration;

        private readonly IHubContext<SupportConversationHub> _hubContext;
        public WebhookService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<WebhookService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IProviderService providerService, ICustomerProfileService customerProfileService, ICustomerMessageService customerMessageService, IZaloUserService zaloUserService, IFacebookUserService facebookUserService, IConfiguration configuration, IInstagramUserService instagramUserService, IHubContext<SupportConversationHub> hubContext, ISupportConversationService supportConversationService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _providerService = providerService;
            _customerProfileService = customerProfileService;
            _customerMessageService = customerMessageService;
            _zaloUserService = zaloUserService;
            _facebookUserService = facebookUserService;
            _configuration = configuration;
            _instagramUserService = instagramUserService;
            _hubContext = hubContext;
            _supportConversationService = supportConversationService;
        }

        //========== Zalo //==========
        public async Task<bool> ZaloWebhookAsync(ZaloWebhookEvent zaloEvent)
        {
            bool result = false;

            Guid conversationId = Guid.Empty;

            _logger.LogInformation(
                "[ZALO] Webhook received | EventName={EventName} | SenderId={SenderId} | Timestamp={Timestamp}",
                zaloEvent?.EventName,
                zaloEvent?.Sender?.Id,
                zaloEvent?.Timestamp
            );

            if (zaloEvent == null)
            {
                _logger.LogWarning("[ZALO] Payload is NULL");
                return false;
            }

            if (zaloEvent.EventName != "user_send_text")
            {
                _logger.LogInformation(
                    "[ZALO] Ignored event | EventName={EventName}",
                    zaloEvent.EventName
                );
                return true;
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
            var customerProfile =
                await _customerProfileService.GetCustomerProfileBySenderAsync(zaloEvent.Sender.Id.ToString());


            if (customerProfile == null)
            {
                _logger.LogInformation(
                    "[ZALO] CustomerProfile not found | SenderId={SenderId} → Creating new",
                    zaloEvent.Sender.Id
                );

                var zaloProfile =
                    await _zaloUserService.GetUserProfileAsync(zaloEvent.Sender.Id);

                customerProfile =
                    await _customerProfileService.CreateCustomerProfileAsync(
                        new CreateCustomerProfileRequest
                        {
                            ZaloSenderId = zaloEvent.Sender.Id.ToString(),
                            CustomerName =
                                zaloProfile?.DisplayName
                                ?? $"Zalo User {zaloEvent.Sender.Id}",
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
            // find pending Conversation 

            var existConversation = await _supportConversationService.GetSupportConversationHavePendingByCustomerIdAsync(customerProfile.Id, provider.Id);

            if (existConversation != null)
            {
                conversationId = existConversation.Id;

            }
            else
            {
                // if no have pending conversation
                var newSupportConversation = new CreateSupportConversationRequest
                {
                    ActiveCustomerId = customerProfile.Id,
                    ActiveStaffId = null,
                    AvatarUrl = customerProfile.AvatarUrl,
                    CustomerName = customerProfile.CustomerName,
                    IsDistributed = true,
                    ProvidersId = provider.Id,
                    Status = ConversationStatus.Pending,

                };
                try
                {
                    // dung cho truong hop 2 tin nhan gui toi cung luc 
                    var newComversation = await _supportConversationService.CreateNewSupportConversationAsync(newSupportConversation);

                    conversationId = newComversation.Id;
                }
                catch(DbUpdateException)
{
                    var checkExistConversation =
                        await _supportConversationService
                            .GetSupportConversationHavePendingByCustomerIdAsync(
                                customerProfile.Id,
                                provider.Id);

                    if (checkExistConversation == null)
                        throw;

                    conversationId = checkExistConversation.Id;
                }
            }

            var newMessage = await _customerMessageService.CreateCustomerMessageAsync(
                new CreateCustomerMessageRequest
                {
                    Content = zaloEvent.Message?.Text,
                    Timestamp = zaloEvent.Timestamp,
                    KeywordActive = false,
                    CustomerId = customerProfile.Id,
                    ConversationId = conversationId,
                }
            );

            var conversation = await _supportConversationService.GetSupportConversationByIdAsync(conversationId);

            if (conversation.ActiveStaffId == null)
            {
                // asign Staff after run distribute

                Guid StaffId = Guid.Parse("89ceebe8-4ee8-4bf0-8893-977978dbc9e6");

                var AsignConversationSupport = await _supportConversationService.AsignForSupportConversationByIdAsync(conversationId, StaffId);
            }

            if (newMessage != null)
            {
                _logger.LogInformation(
                    "[ZALO] Message created | MessageId={MessageId}",
                    newMessage.Id
                );
                result = true;
                // After get new customer message Update Supportconversation UpdateDate -> now
                var updatedConversation = await _supportConversationService.UpdateSupportConversationUpdateDateAsync(conversationId);

                if (updatedConversation.ActiveStaffId != null)
                {
                    // Add SignalR Realtime for sidebar staff 
                    await _hubContext.Clients.User(updatedConversation.ActiveStaffId.ToString())
                        .SendAsync("SidebarUpdated", new StaffConversationSideBarUpdateResponse
                        {
                            ConversationId = updatedConversation.Id,
                            CustomerName = updatedConversation.CustomerName,
                            avartarUrl = updatedConversation.AvatarUrl,
                            providerName = provider.ProviderName,
                            LastMessage = newMessage.Content,
                            MessageUpdateDate = updatedConversation.UpdateDate
                        });

                    // Add SignalR realTime for chat detail if staff is viewing
                    await _hubContext.Clients.Group($"conversation:{updatedConversation.Id}")
                        .SendAsync("ReceiveMessage", new SupportConversationMessagesResponse
                        {
                            SenderType = "Customer",
                            SenderId = customerProfile.Id,
                            Content = newMessage.Content,
                            Timestamp = newMessage.Timestamp
                        });
                }
            }
            else
            {
                _logger.LogError(
                    "[Zalo] Failed to create message | SenderId={SenderId}",
                    customerProfile.ZaloSenderId
                );
            }
            return result;
        }


        //========== Facebook //==========

        public async Task<bool> FacebookWebhookAsync(FaceBookWebhookPayload faceBookWebhookPayload)
        {
           

            _logger.LogInformation("[FACEBOOK] Webhook received");

            if (faceBookWebhookPayload?.entry == null ||
                !faceBookWebhookPayload.entry.Any())
            {
                _logger.LogWarning("[FACEBOOK] Payload invalid or empty entry");
                return false;
            }

            var provider =
                await _providerService.GetProviderByNameAsync("Facebook")
                ?? throw new BusinessException("Provider Facebook Not found");

            _logger.LogInformation(
                "[FACEBOOK] Provider loaded | ProviderId={ProviderId}",
                provider.Id
            );

            bool result = false;

            foreach (var entry in faceBookWebhookPayload.entry)
            {
                var conversationId = Guid.Empty;

                if (entry.messaging == null)
                {
                    _logger.LogWarning("[FACEBOOK] Entry has no messages");
                    continue;
                }

                foreach (var messaging in entry.messaging)
                {
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

                    var customerProfile =
                        await _customerProfileService.GetCustomerProfileBySenderAsync(
                            senderId: messaging.sender.id
                        );

                    if (customerProfile == null)
                    {
                        _logger.LogInformation(
                            "[FACEBOOK] CustomerProfile not found | SenderId={SenderId} → Creating new",
                            messaging.sender.id
                        );

                        var fbUser =
                            await _facebookUserService.GetUserProfileAsync(
                                messaging.sender.id
                            );

                        customerProfile =
                            await _customerProfileService.CreateCustomerProfileAsync(
                                new CreateCustomerProfileRequest
                                {
                                    FacebookSenderId = messaging.sender.id,
                                    CustomerName =
                                        $"{fbUser?.FirstName} {fbUser?.LastName}".Trim(),
                                    AvatarUrl = fbUser?.ProfilePic,
                                }
                            );

                        _logger.LogInformation(
                            "[FACEBOOK] CustomerProfile created | CustomerId={CustomerId}",
                            customerProfile.Id
                        );
                    }

                    var existConversation = await _supportConversationService.GetSupportConversationHavePendingByCustomerIdAsync(customerProfile.Id, provider.Id);

                    if (existConversation != null)
                    {
                        conversationId = existConversation.Id;

                    }
                    else
                    {
                        // if no have pending conversation
                        var newSupportConversation = new CreateSupportConversationRequest
                        {
                            ActiveCustomerId = customerProfile.Id,
                            ActiveStaffId = null,
                            AvatarUrl = customerProfile.AvatarUrl,
                            CustomerName = customerProfile.CustomerName,
                            IsDistributed = true,
                            ProvidersId = provider.Id,
                            Status = ConversationStatus.Pending,

                        };
                        try
                        {
                            // dung cho truong hop 2 tin nhan gui toi cung luc 
                            var newComversation = await _supportConversationService.CreateNewSupportConversationAsync(newSupportConversation);

                            conversationId = newComversation.Id;
                        }
                        catch (DbUpdateException)
                        {
                            var checkExistConversation =
                                await _supportConversationService
                                    .GetSupportConversationHavePendingByCustomerIdAsync(
                                        customerProfile.Id,
                                        provider.Id);

                            if (checkExistConversation == null)
                                throw;

                            conversationId = checkExistConversation.Id;
                        }
                    
                    }

                    var newMessage =
                        await _customerMessageService.CreateCustomerMessageAsync(
                            new CreateCustomerMessageRequest
                            {
                                Content = messaging.message.text,
                                Timestamp = messaging.timestamp,
                                KeywordActive = false,
                                CustomerId = customerProfile.Id,
                                ConversationId = conversationId,

                            }
                        );
                    var conversation = await _supportConversationService.GetSupportConversationByIdAsync(conversationId);
                   
                    if (conversation.ActiveStaffId == null)
                    {

                        // asign Staff after run distribute

                        Guid StaffId = Guid.Parse("89ceebe8-4ee8-4bf0-8893-977978dbc9e6");

                        var AsignConversationSupport = await _supportConversationService.AsignForSupportConversationByIdAsync(conversationId, StaffId);
                    }

                    if (newMessage != null)
                    {
                        _logger.LogInformation(
                            "[FACEBOOK] Message created | MessageId={MessageId}",
                            newMessage.Id
                        );
                        result = true;
                        // After get new customer message Update Supportconversation UpdateDate -> now
                        var updatedConversation = await _supportConversationService.UpdateSupportConversationUpdateDateAsync(conversationId);

                        if (updatedConversation.ActiveStaffId != null)
                        {
                            // Add SignalR Realtime for sidebar staff 
                            await _hubContext.Clients.User(updatedConversation.ActiveStaffId.ToString())
                                .SendAsync("SidebarUpdated", new StaffConversationSideBarUpdateResponse
                                {
                                    ConversationId = updatedConversation.Id,
                                    CustomerName = updatedConversation.CustomerName,
                                    avartarUrl = updatedConversation.AvatarUrl,
                                    providerName = provider.ProviderName,
                                    LastMessage = newMessage.Content,
                                    MessageUpdateDate = updatedConversation.UpdateDate
                                });

                            // Add SignalR realTime for chat detail if staff is viewing
                            await _hubContext.Clients.Group($"conversation:{updatedConversation.Id}")
                                .SendAsync("ReceiveMessage", new SupportConversationMessagesResponse
                                {
                                    SenderType = "Customer",
                                    SenderId = customerProfile.Id,
                                    Content = newMessage.Content,
                                    Timestamp = newMessage.Timestamp
                                });
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

            return result;
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

        public async Task<bool> InstagramWebhookAsync(InstagramWebhookPayload payload)
        {
            const string ourSenderId = "17841478357005004"; // if message is our past continue next message
            _logger.LogInformation("[INSTAGRAM] Webhook received");

            _logger.LogInformation(
                "[INSTAGRAM] RAW PAYLOAD:\n{Payload}",
                JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true })
            );

            if (payload?.entry == null || !payload.entry.Any())
            {
                _logger.LogWarning("[INSTAGRAM] Payload invalid or empty entry");
                return false;
            }

            var provider =
                await _providerService.GetProviderByNameAsync("Instagram")
                ?? throw new BusinessException("Provider Instagram Not found");

            bool result = false;

            foreach (var entry in payload.entry)
            {
                var conversationId = Guid.Empty;

                if (entry.messaging == null || !entry.messaging.Any())
                {
                    _logger.LogWarning(
                        "[INSTAGRAM] Entry has no messaging | BusinessId={BusinessId}",
                        entry.id
                    );
                    continue;
                }

                foreach (var msg in entry.messaging)
                {
                    if (msg.Sender.id == ourSenderId)
                    {
                        _logger.LogInformation(
                            "[INSTAGRAM] Ignored staff message | BusinessId={BusinessId} | SenderId={SenderId}",
                            entry.id,
                            msg.Sender?.id
                        );
                        continue;
                    }

                    if (msg.Message?.text == null)
                    {
                        _logger.LogInformation(
                            "[INSTAGRAM] Ignored non-text message | BusinessId={BusinessId} | SenderId={SenderId}",
                            entry.id,
                            msg.Sender?.id
                        );
                        continue;
                    }

                    var businessId = entry.id;      // Instagram Business
                    var senderId = msg.Sender.id;  // Customer
                    var text = msg.Message.text;

                    _logger.LogInformation(
                        "[INSTAGRAM] Message received | BusinessId={BusinessId} | SenderId={SenderId} | Text={Text}",
                        businessId,
                        senderId,
                        text
                    );

                    // ==== BUSINESS LOGIC ====

                    var customerProfile =
                        await _customerProfileService.GetCustomerProfileBySenderAsync(
                            senderId: senderId
                        );

                    if (customerProfile == null)
                    {
                        _logger.LogInformation(
                            "[INSTAGRAM] CustomerProfile not found | SenderId={SenderId} → Creating new",
                            senderId
                        );

                        var igUser = await _instagramUserService.GetUserProfileAsync(senderId);

                        customerProfile =
                            await _customerProfileService.CreateCustomerProfileAsync(
                                new CreateCustomerProfileRequest
                                {
                                    InstagramSenderId = senderId,
                                    CustomerName = igUser?.Name ?? "Instagram User",
                                    AvatarUrl = igUser?.ProfilePictureUrl,
                                }
                            );
                    }


                    var existConversation = await _supportConversationService.GetSupportConversationHavePendingByCustomerIdAsync(customerProfile.Id, provider.Id);

                    if (existConversation != null)
                    {
                        conversationId = existConversation.Id;

                    }
                    else
                    {
                        // if no have pending conversation
                        var newSupportConversation = new CreateSupportConversationRequest
                        {
                            ActiveCustomerId = customerProfile.Id,
                            ActiveStaffId = null,
                            AvatarUrl = customerProfile.AvatarUrl,
                            CustomerName = customerProfile.CustomerName,
                            IsDistributed = true,
                            ProvidersId = provider.Id,
                            Status = ConversationStatus.Pending,

                        };
                        try
                        {
                            // dung cho truong hop 2 tin nhan gui toi cung luc 
                            var newComversation = await _supportConversationService.CreateNewSupportConversationAsync(newSupportConversation);

                            conversationId = newComversation.Id;
                        }
                        catch (DbUpdateException)
                        {
                            var checkExistConversation =
                                await _supportConversationService
                                    .GetSupportConversationHavePendingByCustomerIdAsync(
                                        customerProfile.Id,
                                        provider.Id);

                            if (checkExistConversation == null)
                                throw;

                            conversationId = checkExistConversation.Id;
                        }

                      
                    }

                    var newMessage =
                        await _customerMessageService.CreateCustomerMessageAsync(
                            new CreateCustomerMessageRequest
                            {
                                Content = text,
                                Timestamp = msg.Timestamp,
                                KeywordActive = false,
                                CustomerId = customerProfile.Id,
                                ConversationId = conversationId,
                            }
                        );

                    var conversation = await _supportConversationService.GetSupportConversationByIdAsync(conversationId);

                    if (conversation.ActiveStaffId == null)
                    {

                        // asign Staff after run distribute

                        Guid StaffId = Guid.Parse("89ceebe8-4ee8-4bf0-8893-977978dbc9e6");

                        var AsignConversationSupport = await _supportConversationService.AsignForSupportConversationByIdAsync(conversationId, StaffId);
                    }


                    if (newMessage != null)
                    {
                        _logger.LogInformation(
                            "[INSTAGRAM] Message created | MessageId={MessageId}",
                            newMessage.Id
                        );
                        result = true;

                        // After get new customer message Update Supportconversation UpdateDate -> now
                        var updatedConversation = await _supportConversationService.UpdateSupportConversationUpdateDateAsync(conversationId);

                        if (updatedConversation.ActiveStaffId != null)
                        {
                            // Add SignalR Realtime for sidebar staff 
                            await _hubContext.Clients.User(updatedConversation.ActiveStaffId.ToString())
                                .SendAsync("SidebarUpdated", new StaffConversationSideBarUpdateResponse
                                {
                                    ConversationId = updatedConversation.Id,
                                    CustomerName = updatedConversation.CustomerName,
                                    avartarUrl = updatedConversation.AvatarUrl,
                                    providerName = provider.ProviderName,
                                    LastMessage = newMessage.Content,
                                    MessageUpdateDate = updatedConversation.UpdateDate
                                });

                            // Add SignalR realTime for chat detail if staff is viewing
                            await _hubContext.Clients.Group($"conversation:{updatedConversation.Id}")
                                .SendAsync("ReceiveMessage", new SupportConversationMessagesResponse
                                {
                                    SenderType = "Customer",
                                    SenderId = customerProfile.Id,
                                    Content = newMessage.Content,
                                    Timestamp = newMessage.Timestamp
                                });
                        }
                    }
                }
            }

            return result;
        }


        public async Task<bool> VerifyInstagramWebhook(string mode, string token)
        {
            _logger.LogInformation(
                "[INSTAGRAM][VERIFY] Verify webhook called | Mode={Mode} | Token={Token}",
                mode,
                string.IsNullOrEmpty(token) ? "NULL" : "REDACTED"
            );

            var verifyToken = _configuration["InstagramWebhook:verifyToken"];

            if (string.IsNullOrEmpty(verifyToken))
            {
                _logger.LogError(
                    "[INSTAGRAM][VERIFY] VerifyToken not found in configuration (InstagramWebhook:verifyToken)"
                );
                return false;
            }

            if (mode != "subscribe")
            {
                _logger.LogWarning(
                    "[INSTAGRAM][VERIFY] Invalid mode | Expected=subscribe | Actual={Mode}",
                    mode
                );
                return false;
            }

            if (token != verifyToken)
            {
                _logger.LogWarning(
                    "[INSTAGRAM][VERIFY] Token mismatch | Provided={Provided} | Expected={Expected}",
                    "REDACTED",
                    "REDACTED"
                );
                return false;
            }

            _logger.LogInformation(
                "[INSTAGRAM][VERIFY] Webhook verification SUCCESS"
            );

            return true;
        }

    }
}
