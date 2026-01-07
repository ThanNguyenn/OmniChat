using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Webhooks.Facebook.WebhookMessage;
using OmniChat.Application.Webhooks.Zalo.WebhookMessage;
using OmniChat.Infrastructure.Dtos.Requests.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Requests.Provider;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

        private readonly IConfiguration _configuration;
        public WebhookService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<WebhookService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IProviderService providerService, ICustomerProfileService customerProfileService, ICustomerMessageService customerMessageService, IZaloUserService zaloUserService, IFacebookUserService facebookUserService,IConfiguration configuration) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _providerService = providerService;
            _customerProfileService = customerProfileService;
            _customerMessageService = customerMessageService;
            _zaloUserService = zaloUserService;
            _facebookUserService = facebookUserService;
            _configuration = configuration;
        }

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
        public async Task<bool> ZaloWebhookAsync(ZaloWebhookEvent zaloEvent)
        {
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
            var provider = await _providerService.GetProviderAsync("Zalo");
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
                await _customerProfileService.GetCustomerProfileBySenderAndProviderIdIdAsync(
                    senderId: zaloEvent.Sender.Id,
                    providersId: provider.Id
                );

            if (customerProfile == null)
            {
                _logger.LogInformation(
                    "[ZALO] CustomerProfile not found | SenderId={SenderId} → Creating new",
                    zaloEvent.Sender.Id
                );

                var zaloProfile =
                    await _zaloUserService.GetUserProfileAsync(zaloEvent.Sender.Id);

                customerProfile =
                    await _customerProfileService.CreateCustomerProfileEntityAsync(
                        new CreateCustomerProfileRequest
                        {
                            SenderId = zaloEvent.Sender.Id,
                            ProvidersId = provider.Id,
                            CustomerName =
                                zaloProfile?.DisplayName
                                ?? $"Zalo User {zaloEvent.Sender.Id}",
                            AvatarUrl = zaloProfile?.Avatar,
                            PhoneNumber = zaloProfile?.SharedInfo?.Phone,
                            Gender = zaloProfile?.Gender == 1,
                            DateOfBirth = _zaloUserService.ParseZaloBirthDate(
                                zaloProfile?.BirthDate
                            )
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

            Guid ConversationTempId = Guid.Parse("55555555-5555-5555-5555-555555555555");

            var messageRequest = new CreateCustomerMessageRequest
            {
                Content = zaloEvent.Message?.Text,
                Timestamp = zaloEvent.Timestamp,
                KeywordActive = false,
                CustomerId = customerProfile.Id,
                ConversationId = ConversationTempId
            };

            _logger.LogInformation(
                "[ZALO] Creating message | CustomerId={CustomerId} | Content={Content}",
                customerProfile.Id,
                messageRequest.Content
            );

            var newCustomerMess =
                await _customerMessageService.CreateCustomerMessageAsync(messageRequest);

            if (newCustomerMess == null)
            {
                _logger.LogError(
                    "[ZALO] Failed to create message | CustomerId={CustomerId}",
                    customerProfile.Id
                );
                return false;
            }

            _logger.LogInformation(
                "[ZALO] Message created successfully | MessageId={MessageId}",
                newCustomerMess.Id
            );

            return true;
        }


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
                if (entry.facebookMessages == null)
                {
                    _logger.LogWarning("[FACEBOOK] Entry has no messages");
                    continue;
                }

                foreach (var messaging in entry.facebookMessages)
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
                                    SenderId = messaging.sender.id,
                                    CustomerName =
                                        $"{fbUser?.FirstName} {fbUser?.LastName}".Trim(),
                                    ProvidersId = provider.Id,
                                    AvatarUrl = fbUser?.ProfilePic,
                                    Gender = fbUser?.Gender == "male",
                                    Email = null,
                                    PhoneNumber = null,
                                    DateOfBirth = null
                                }
                            );

                        _logger.LogInformation(
                            "[FACEBOOK] CustomerProfile created | CustomerId={CustomerId}",
                            customerProfile.Id
                        );
                    }

                    Guid ConversationTempId =
                        Guid.Parse("55555555-5555-5555-5555-555555555555");

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

        public async Task<bool> VerifyWebhook(string mode, string token)
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

    }
}
