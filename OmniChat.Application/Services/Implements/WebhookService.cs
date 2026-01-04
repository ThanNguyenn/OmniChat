using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Webhooks.Zalo.WebhookMessage;
using OmniChat.Infrastructure.Dtos.Requests.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Requests.Provider;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class WebhookService : BaseService<WebhookService>
    {
        private readonly IProviderService _providerService;

        private readonly ICustomerProfileService _customerProfileService;

        private readonly ICustomerMessageService _customerMessageService;

        public WebhookService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<WebhookService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IProviderService providerService, ICustomerProfileService customerProfileService, ICustomerMessageService customerMessageService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _providerService = providerService;
            _customerProfileService = customerProfileService;
            _customerMessageService = customerMessageService;
        }

        //public async Task<bool> ZaloWebhookAsync(ZaloWebhookEvent zaloEvent)
        //{
        //    try
        //    {
        //        if (zaloEvent == null)
        //            return false;

        //        if (zaloEvent.EventName != "user_send_text")
        //            return true; // ignore event khác

        //        // 1️⃣ Get Provider Zalo
        //        var provider = await _providerService.GetProviderAsync("Zalo");
        //        if (provider == null)
        //            throw new BusinessException("Provider Zalo not found");

        //        // 2️⃣ Get CustomerProfile theo SenderId + Provider
        //        var customerProfile =
        //            await _customerProfileService
        //                .GetCustomerProfileBySenderIdAsync(
        //                    zaloEvent.Sender.Id
        //                );

               
        //        if (customerProfile == null)
        //        {
        //            customerProfile =
        //                await _customerProfileService
        //                    .CreateNewCustomerProfileAsync(
        //                        new CreateCustomerProfileRequest
        //                        {
        //                            SenderId = zaloEvent.Sender.Id,
                                   
        //                        }
        //                    );
        //        }

                
        //        var messageRequest = new CreateCustomerMessageRequest
        //        {
        //            Content = zaloEvent.Message?.Text,
        //            Timestamp = zaloEvent.Timestamp, 
        //            KeywordActive = false,
        //            CustomerId = customerProfile.Id,
        //            ConversationId = Guid.Empty 
        //        };

        //        await _customerMessageService.CreateCustomerMessageAsync(messageRequest);

        //        return true;

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error get Webhook :{Message}.", ex.Message);
        //        throw;
        //    }
        //}
    }
}
