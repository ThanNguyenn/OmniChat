using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
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
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Exceptions;
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
        public WebhookService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<WebhookService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IProviderService providerService, ICustomerProfileService customerProfileService, ICustomerMessageService customerMessageService, IZaloUserService zaloUserService, IFacebookUserService facebookUserService,IConfiguration configuration, IInstagramUserService instagramUserService,IHubContext<SupportConversationHub> hubContext, ISupportConversationService supportConversationService) : base(unitOfWork, logger, mapper, httpContextAccessor)
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

        //public async Task<bool> ZaloWebhookAsync(ZaloWebhookEvent zaloEvent)
        //{

        //        if (zaloEvent == null)
        //            return false;

        //        if (zaloEvent.EventName != "user_send_text")
        //            return true;

        //        //  Provider Zalo
        //        var provider = await _providerService.GetProviderAsync("Zalo")
        //            ?? throw new BusinessException("Provider Zalo not found");

        //        //  Get CustomerProfile theo SenderId + ProviderId
        //        var customerProfile =
        //            await _customerProfileService
        //                .GetCustomerProfileBySenderAndProviderIdIdAsync(
        //                    senderId: zaloEvent.Sender.Id,
        //                    providersId: provider.Id
        //                );

        //        //  if don't have profile => create new
        //        if (customerProfile == null)
        //        {

        //            var zaloProfile =
        //                await _zaloUserService.GetUserProfileAsync(zaloEvent.Sender.Id);

        //            customerProfile =
        //                await _customerProfileService.CreateCustomerProfileEntityAsync(
        //                    new CreateCustomerProfileRequest
        //                    {
        //                        SenderId = zaloEvent.Sender.Id,
        //                        ProvidersId = provider.Id,

        //                        CustomerName =
        //                            zaloProfile?.DisplayName
        //                            ?? $"Zalo User {zaloEvent.Sender.Id}",

        //                        AvatarUrl = zaloProfile?.Avatar,
        //                        PhoneNumber = zaloProfile?.SharedInfo?.Phone,

        //                        Gender = zaloProfile?.Gender == 1,
        //                        DateOfBirth = _zaloUserService.ParseZaloBirthDate(
        //                            zaloProfile?.BirthDate
        //                        )
        //                    }
        //                );
        //        }

        //        Guid ConversationTempId = Guid.Parse("55555555-5555-5555-5555-555555555555");


        //        var messageRequest = new CreateCustomerMessageRequest
        //        {
        //            Content = zaloEvent.Message?.Text,
        //            Timestamp = zaloEvent.Timestamp,
        //            KeywordActive = false,
        //            CustomerId = customerProfile.Id,
        //            ConversationId = ConversationTempId // just temp -> this will have after done atribute funton
        //        };

        //       var newCustomerMess = await _customerMessageService.CreateCustomerMessageAsync(messageRequest);

        //        if(newCustomerMess == null)
        //        {
        //            return false;
        //        }

        //        return true;      
        //}
        //public async Task<bool> ZaloWebhookAsync(ZaloWebhookEvent zaloEvent)
        //{
        //    _logger.LogInformation(
        //        "[ZALO] Webhook received | EventName={EventName} | SenderId={SenderId} | Timestamp={Timestamp}",
        //        zaloEvent?.EventName,
        //        zaloEvent?.Sender?.Id,
        //        zaloEvent?.Timestamp
        //    );

        //    if (zaloEvent == null)
        //    {
        //        _logger.LogWarning("[ZALO] Payload is NULL");
        //        return false;
        //    }

        //    if (zaloEvent.EventName != "user_send_text")
        //    {
        //        _logger.LogInformation(
        //            "[ZALO] Ignored event | EventName={EventName}",
        //            zaloEvent.EventName
        //        );
        //        return true;
        //    }

        //    // Get Provider
        //    var provider = await _providerService.GetProviderAsync("Zalo");
        //    if (provider == null)
        //    {
        //        _logger.LogError("[ZALO] Provider Zalo not found");
        //        throw new BusinessException("Provider Zalo not found");
        //    }

        //    _logger.LogInformation(
        //        "[ZALO] Provider loaded | ProviderId={ProviderId}",
        //        provider.Id
        //    );

        //    // Get CustomerProfile
        //    var customerProfile =
        //        await _customerProfileService.GetCustomerProfileBySenderAndProviderIdIdAsync(
        //            senderId: zaloEvent.Sender.Id,
        //            providersId: provider.Id
        //        );

        //    if (customerProfile == null)
        //    {
        //        _logger.LogInformation(
        //            "[ZALO] CustomerProfile not found | SenderId={SenderId} → Creating new",
        //            zaloEvent.Sender.Id
        //        );

        //        var zaloProfile =
        //            await _zaloUserService.GetUserProfileAsync(zaloEvent.Sender.Id);

        //        customerProfile =
        //            await _customerProfileService.CreateCustomerProfileEntityAsync(
        //                new CreateCustomerProfileRequest
        //                {
        //                    SenderId = zaloEvent.Sender.Id,
        //                    ProvidersId = provider.Id,
        //                    CustomerName =
        //                        zaloProfile?.DisplayName
        //                        ?? $"Zalo User {zaloEvent.Sender.Id}",
        //                    AvatarUrl = zaloProfile?.Avatar,
        //                    PhoneNumber = zaloProfile?.SharedInfo?.Phone,
        //                    Gender = zaloProfile?.Gender == 1,
        //                    DateOfBirth = _zaloUserService.ParseZaloBirthDate(
        //                        zaloProfile?.BirthDate
        //                    )
        //                }
        //            );

        //        _logger.LogInformation(
        //            "[ZALO] CustomerProfile created | CustomerId={CustomerId}",
        //            customerProfile.Id
        //        );
        //    }
        //    else
        //    {
        //        _logger.LogInformation(
        //            "[ZALO] CustomerProfile found | CustomerId={CustomerId}",
        //            customerProfile.Id
        //        );
        //    }

        //    Guid ConversationTempId = Guid.Parse("ad07c5a4-55aa-4708-aeaf-cc9de6fb089e");

        //    var messageRequest = new CreateCustomerMessageRequest
        //    {
        //        Content = zaloEvent.Message?.Text,
        //        Timestamp = zaloEvent.Timestamp,
        //        KeywordActive = false,
        //        CustomerId = customerProfile.Id,
        //        ConversationId = ConversationTempId
        //    };

        //    _logger.LogInformation(
        //        "[ZALO] Creating message | CustomerId={CustomerId} | Content={Content}",
        //        customerProfile.Id,
        //        messageRequest.Content
        //    );

        //    var newCustomerMess =
        //        await _customerMessageService.CreateCustomerMessageAsync(messageRequest);

        //    if (newCustomerMess == null)
        //    {
        //        _logger.LogError(
        //            "[ZALO] Failed to create message | CustomerId={CustomerId}",
        //            customerProfile.Id
        //        );
        //        return false;
        //    }

        //    _logger.LogInformation(
        //        "[ZALO] Message created successfully | MessageId={MessageId}",
        //        newCustomerMess.Id
        //    );

        //    return true;
        //}

                                 //========== Facebook //==========

        //public async Task<bool> FacebookWebhookAsync(FaceBookWebhookPayload faceBookWebhookPayload)
        //{
        //   bool result = false;

        //        if(faceBookWebhookPayload?.FacebookEntry == null || !faceBookWebhookPayload.FacebookEntry.Any())
        //        {
        //            return false;
        //        }

        //        // get provider 

        //        var provider = await _providerService.GetProviderAsync("Facebook") ?? throw new BusinessException("Provider Facebook Not found");


        //        //  Get CustomerProfile theo SenderId + ProviderId

        //        foreach (var entry in faceBookWebhookPayload.FacebookEntry)
        //        {

        //            if(entry.facebookMessages == null)
        //                continue;

        //            foreach (var messaging in entry.facebookMessages)
        //            {
        //                // now just message text 
        //                if(messaging.message?.text == null)
        //                    continue;

        //                // Get or Create CustomerProfile
        //                var customerProfile = await _customerProfileService.GetCustomerProfileBySenderAndProviderIdIdAsync(
        //                    senderId: messaging.sender.id,
        //                    providersId: provider.Id
        //                    );

        //                if (customerProfile == null)
        //                {
        //                    // create if don't exit

        //                    var fbUser = await _facebookUserService.GetUserProfileAsync(messaging.sender.id);

        //                    var CustomerName = $"{fbUser?.FirstName} {fbUser?.LastName}".Trim();

        //                    bool customerGender = fbUser?.Gender == "male";


        //                    customerProfile = await _customerProfileService.CreateCustomerProfileEntityAsync(new CreateCustomerProfileRequest
        //                    {
        //                        SenderId = messaging.sender.id,
        //                        CustomerName = CustomerName,
        //                        ProvidersId = provider.Id,
        //                        AvatarUrl = fbUser?.ProfilePic,
        //                        Gender = customerGender,
        //                        Email = null,
        //                        PhoneNumber = null,
        //                        DateOfBirth = null,
        //                    });
        //                }

        //                // create Message 

        //                // conversation temp
        //                Guid ConversationTempId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        //              var newMessage =  await _customerMessageService.CreateCustomerMessageAsync(
        //                    new CreateCustomerMessageRequest
        //                    {
        //                        Content = messaging.message.text,
        //                        Timestamp = messaging.timestamp,
        //                        KeywordActive = false,
        //                        CustomerId = customerProfile.Id,
        //                        ConversationId = ConversationTempId
        //                    }
        //              );

        //                if(newMessage != null)
        //                {
        //                result = true;
        //                }
        //            }
        //        }
        //    return result;
        //}

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
                await _providerService.GetProviderAsync("Facebook")
                ?? throw new BusinessException("Provider Facebook Not found");

            _logger.LogInformation(
                "[FACEBOOK] Provider loaded | ProviderId={ProviderId}",
                provider.Id
            );

            bool result = false;

            foreach (var entry in faceBookWebhookPayload.entry)
            {
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
                        await _customerProfileService.GetCustomerProfileBySenderAndProviderIdIdAsync(
                            senderId: messaging.sender.id,
                            providersId: provider.Id
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
                            await _customerProfileService.CreateCustomerProfileEntityAsync(
                                new CreateCustomerProfileRequest
                                {
                                    FacebookSenderId = messaging.sender.id,
                                    CustomerName =
                                        $"{fbUser?.FirstName} {fbUser?.LastName}".Trim(),
                                    ProvidersId = provider.Id,
                                    AvatarUrl = fbUser?.ProfilePic,
                                }
                            );

                        _logger.LogInformation(
                            "[FACEBOOK] CustomerProfile created | CustomerId={CustomerId}",
                            customerProfile.Id
                        );
                    }

                    Guid ConversationTempId =
                        Guid.Parse("cba10005-e594-47e2-a9e4-4a11d82167ce");

                    var newMessage =
                        await _customerMessageService.CreateCustomerMessageAsync(
                            new CreateCustomerMessageRequest
                            {
                                Content = messaging.message.text,
                                Timestamp = messaging.timestamp,
                                KeywordActive = false,
                                CustomerId = customerProfile.Id,
                                ConversationId = ConversationTempId
                            }
                        );

                    if (newMessage != null)
                    {
                        _logger.LogInformation(
                            "[FACEBOOK] Message created | MessageId={MessageId}",
                            newMessage.Id
                        );
                        result = true;
                        // After get new customer message Update Supportconversation UpdateDate -> now
                        var existconversation =  await _supportConversationService.UpdateSupportConversationUpdateDateAsync(ConversationTempId);

                        // Add SignalR Realtime for sidebar staff 
                        await _hubContext.Clients.User(existconversation.ActiveStaffId.ToString())
                            .SendAsync("SidebarUpdated", new StaffConversationSideBarUpdateResponse
                            {
                            ConversationId = existconversation.Id,
                            CustomerName = existconversation.CustomerName,
                            avartarUrl = existconversation.AvatarUrl,
                            providerName = provider.ProviderName,
                            LastMessage = newMessage.Content,
                            MessageUpdateDate = existconversation.UpdateDate
                        });

                        // Add SignalR realTime for chat detail if staff is viewing
                        await _hubContext.Clients.Group($"conversation:{existconversation.Id}")
                            .SendAsync("ReceiveMessage", new SupportConversationMessagesResponse
                            {
                                SenderType = "Customer",
                                SenderId = customerProfile.Id,
                                Content = newMessage.Content,
                                Timestamp = newMessage.Timestamp
                            });
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


        //public async Task<bool> VerifyWebhook(string mode, string token)
        //{
        //    var verifyToken = _configuration["facebookWebHook:verifyToken"];
        //    return mode == "subscribe" && token == verifyToken;
        //}

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
        //        await _providerService.GetProviderAsync("Instagram")
        //        ?? throw new BusinessException("Provider Instagram Not found");

        //    bool result = false;

        //    foreach (var entry in payload.entry)
        //    {
        //        if (entry.messaging == null || !entry.messaging.Any())
        //        {
        //            _logger.LogWarning(
        //                "[INSTAGRAM] Entry has no changes | BusinessId={BusinessId}",
        //                entry.id
        //            );
        //            continue;
        //        }

        //        foreach (var message in entry.messaging)
        //        {
        //            // Instagram Login API chỉ quan tâm messages
        //            if (change.field != "messages")
        //                continue;

        //            var value = change.value;

        //            if (value?.message?.text == null)
        //            {
        //                _logger.LogInformation(
        //                    "[INSTAGRAM] Ignored non-text message | BusinessId={BusinessId} | SenderId={SenderId}",
        //                    entry.id,
        //                    value?.sender?.id
        //                );
        //                continue;
        //            }

        //            var businessId = entry.id;          // Instagram account của bạn
        //            var senderId = value.sender.id;     // Customer
        //            var text = value.message.text;

        //            _logger.LogInformation(
        //                "[INSTAGRAM] Message received | BusinessId={BusinessId} | SenderId={SenderId} | Text={Text}",
        //                businessId,
        //                senderId,
        //                text
        //            );

        //            // ==== BUSINESS LOGIC ====

        //            var customerProfile =
        //                await _customerProfileService.GetCustomerProfileBySenderAndProviderIdIdAsync(
        //                    senderId: senderId,
        //                    providersId: provider.Id
        //                );

        //            if (customerProfile == null)
        //            {
        //                _logger.LogInformation(
        //                    "[INSTAGRAM] CustomerProfile not found | SenderId={SenderId} → Creating new",
        //                    senderId
        //                );

        //                var igUser = await _instagramUserService.GetUserProfileAsync(senderId);

        //                customerProfile =
        //                    await _customerProfileService.CreateCustomerProfileEntityAsync(
        //                        new CreateCustomerProfileRequest
        //                        {
        //                            SenderId = senderId,
        //                            CustomerName = igUser?.Username ?? "Instagram User",
        //                            ProvidersId = provider.Id,
        //                            AvatarUrl = igUser?.ProfilePictureUrl,
        //                            Gender = false
        //                        }
        //                    );
        //            }

        //            Guid conversationTempId =
        //                Guid.Parse("55555555-5555-5555-5555-555555555555");

        //            var newMessage =
        //                await _customerMessageService.CreateCustomerMessageAsync(
        //                    new CreateCustomerMessageRequest
        //                    {
        //                        Content = text,
        //                        Timestamp = long.Parse(value.timestamp),
        //                        KeywordActive = false,
        //                        CustomerId = customerProfile.Id,
        //                        ConversationId = conversationTempId
        //                    }
        //                );

        //            if (newMessage != null)
        //            {
        //                _logger.LogInformation(
        //                    "[INSTAGRAM] Message created | MessageId={MessageId}",
        //                    newMessage.Id
        //                );
        //                result = true;
        //            }
        //        }
        //    }

        //    return result;
        //}

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
                await _providerService.GetProviderAsync("Instagram")
                ?? throw new BusinessException("Provider Instagram Not found");

            bool result = false;

            foreach (var entry in payload.entry)
            {
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
                    if(msg.Sender.id == ourSenderId)
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
                        await _customerProfileService.GetCustomerProfileBySenderAndProviderIdIdAsync(
                            senderId: senderId,
                            providersId: provider.Id
                        );

                    if (customerProfile == null)
                    {
                        _logger.LogInformation(
                            "[INSTAGRAM] CustomerProfile not found | SenderId={SenderId} → Creating new",
                            senderId
                        );

                        var igUser = await _instagramUserService.GetUserProfileAsync(senderId);

                        customerProfile =
                            await _customerProfileService.CreateCustomerProfileEntityAsync(
                                new CreateCustomerProfileRequest
                                {
                                    InstagramSenderId = senderId,
                                    CustomerName = igUser?.Name ?? "Instagram User",
                                    ProvidersId = provider.Id,
                                    AvatarUrl = igUser?.ProfilePictureUrl,
                                }
                            );
                    }

                    Guid conversationTempId =
                        Guid.Parse("eee885ee-eccd-4423-914b-a0823d325368");

                    var newMessage =
                        await _customerMessageService.CreateCustomerMessageAsync(
                            new CreateCustomerMessageRequest
                            {
                                Content = text,
                                Timestamp = msg.Timestamp,
                                KeywordActive = false,
                                CustomerId = customerProfile.Id,
                                ConversationId = conversationTempId
                            }
                        );

                    if (newMessage != null)
                    {
                        _logger.LogInformation(
                            "[INSTAGRAM] Message created | MessageId={MessageId}",
                            newMessage.Id
                        );
                        result = true;

                        // After get new customer message Update Supportconversation UpdateDate -> now
                        var existconversation = await _supportConversationService.UpdateSupportConversationUpdateDateAsync(conversationTempId);

                        // Add SignalR Realtime for sidebar staff 
                        await _hubContext.Clients.User(existconversation.ActiveStaffId.ToString())
                            .SendAsync("SidebarUpdated", new StaffConversationSideBarUpdateResponse
                            {
                                ConversationId = existconversation.Id,
                                CustomerName = existconversation.CustomerName,
                                avartarUrl = existconversation.AvatarUrl,
                                providerName = provider.ProviderName,
                                LastMessage = newMessage.Content,
                                MessageUpdateDate = existconversation.UpdateDate
                            });

                        // Add SignalR realTime for chat detail if staff is viewing
                        await _hubContext.Clients.Group($"conversation:{existconversation.Id}")
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
