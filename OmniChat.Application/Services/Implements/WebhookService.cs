using AutoMapper;
using Microsoft.AspNetCore.Http;
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
        public WebhookService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<WebhookService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IProviderService providerService, ICustomerProfileService customerProfileService, ICustomerMessageService customerMessageService, IZaloUserService zaloUserService, IFacebookUserService facebookUserService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _providerService = providerService;
            _customerProfileService = customerProfileService;
            _customerMessageService = customerMessageService;
            _zaloUserService = zaloUserService;
            _facebookUserService = facebookUserService;
        }

        public async Task<bool> ZaloWebhookAsync(ZaloWebhookEvent zaloEvent)
        {
            try
            {
                if (zaloEvent == null)
                    return false;

                if (zaloEvent.EventName != "user_send_text")
                    return true;

                //  Provider Zalo
                var provider = await _providerService.GetProviderAsync("Zalo")
                    ?? throw new BusinessException("Provider Zalo not found");

                //  Get CustomerProfile theo SenderId + ProviderId
                var customerProfile =
                    await _customerProfileService
                        .GetCustomerProfileBySenderAndProviderIdIdAsync(
                            senderId: zaloEvent.Sender.Id,
                            providersId: provider.Id
                        );

                //  if don't have profile => create new
                if (customerProfile == null)
                {

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
                }

                Guid ConversationTempId = Guid.Parse("55555555-5555-5555-5555-555555555555");


                var messageRequest = new CreateCustomerMessageRequest
                {
                    Content = zaloEvent.Message?.Text,
                    Timestamp = zaloEvent.Timestamp,
                    KeywordActive = false,
                    CustomerId = customerProfile.Id,
                    ConversationId = ConversationTempId // just temp -> this will have after done atribute funton
                };

                await _customerMessageService.CreateCustomerMessageAsync(messageRequest);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Zalo webhook: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> FacebookWebhookAsync(FaceBookWebhookPayload faceBookWebhookPayload)
        {
            try
            {
                if(faceBookWebhookPayload?.FacebookEntry == null || !faceBookWebhookPayload.FacebookEntry.Any())
                {
                    return false;
                }

                // get provider 

                var provider = await _providerService.GetProviderAsync("Facebook") ?? throw new BusinessException("Provider Facebook Not found");


                //  Get CustomerProfile theo SenderId + ProviderId

                foreach (var entry in faceBookWebhookPayload.FacebookEntry)
                {

                    if(entry.facebookMessages == null)
                        continue;

                    foreach (var messaging in entry.facebookMessages)
                    {
                        // now just message text 
                        if(messaging.Message?.Text == null)
                            continue;

                        // Get or Create CustomerProfile
                        var customerProfile = await _customerProfileService.GetCustomerProfileBySenderAndProviderIdIdAsync(
                            senderId: messaging.Sender.Id,
                            providersId: provider.Id
                            );

                        if (customerProfile == null)
                        {
                            // create if don't exit
                            
                            var fbUser = await _facebookUserService.GetUserProfileAsync(messaging.Sender.Id);

                            var CustomerName = $"{fbUser?.FirstName} {fbUser?.LastName}".Trim();

                            bool customerGender = fbUser?.Gender == "male";


                            customerProfile = await _customerProfileService.CreateCustomerProfileEntityAsync(new CreateCustomerProfileRequest
                            {
                                SenderId = messaging.Sender.Id,
                                CustomerName = CustomerName,
                                ProvidersId = provider.Id,
                                AvatarUrl = fbUser?.ProfilePic,
                                Gender = customerGender,
                                Email = null,
                                PhoneNumber = null,
                                DateOfBirth = null,
                            });
                        }

                        // create Message 

                        // conversation temp
                        Guid ConversationTempId = Guid.Parse("55555555-5555-5555-5555-555555555555");

                        await _customerMessageService.CreateCustomerMessageAsync(
                            new CreateCustomerMessageRequest
                            {
                                Content = messaging.Message.Text,
                                Timestamp = messaging.Timestamp,
                                KeywordActive = false,
                                CustomerId = customerProfile.Id,
                                ConversationId = ConversationTempId
                            });
                        }
                }
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Facebook webhook: {Message}", ex.Message);
                throw;
            }
        }
    }
}
