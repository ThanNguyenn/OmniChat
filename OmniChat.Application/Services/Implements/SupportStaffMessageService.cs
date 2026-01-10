using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Webhooks.Zalo.WebhookMessage;
using OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage;
using OmniChat.Infrastructure.Dtos.Responses.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Responses.SupportStaffMessage;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace OmniChat.Application.Services.Implements
{
    public class SupportStaffMessageService : BaseService<SupportStaffMessageService>, ISupportStaffMessageService
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IZaloOAuthService _zaloOAuthService;
        private readonly ICustomerProfileService _customerProfileService;
        private readonly ISupportConversationService _supportConversationService;
        
        public SupportStaffMessageService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<SupportStaffMessageService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IZaloOAuthService zaloOAuthService, ICustomerProfileService customerProfileService, ISupportConversationService supportConversationService, IConfiguration configuration) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _httpClient = httpClient;
            _zaloOAuthService = zaloOAuthService;
            _customerProfileService = customerProfileService;
            _supportConversationService = supportConversationService;
            _configuration = configuration;
        }

        //public async Task SendZaloMessageAsync( CreateSupportStaffMessageRequest newSupportMess)
        //{
        //    // create new support Staff mess
        //    var newStaffSupportMes = await CreateSupportStaffMessageAsync(newSupportMess);

        //    if(newStaffSupportMes == null)
        //    {
        //        throw new Exception("Create fail");
        //    }

        //    // get exist conversation

        //    var existConversation = await _supportConversationService.GetSupportConversationByIdAsync(newStaffSupportMes.SupportConversationId);

        //    if (existConversation == null)
        //    {
        //        throw new NotFoundException("No SupportConversation Found");
        //    }

        //    // get customer profile 

        //    var existCustomerProfile = await _customerProfileService.GetCustomerProfileByIdAsync(existConversation.ActiveCustomerId);

        //    if (existCustomerProfile == null) {
        //        throw new NotFoundException("No existCustomerProfile Found");
        //    }

        //    var accessToken = await _zaloOAuthService.GetAccessTokenAsync();

        //    using var client = new HttpClient();
        //    client.DefaultRequestHeaders.Add("access_token", accessToken);

        //    var payload = new
        //    {
        //        recipient  = new ZaloRecipient { UserId = existCustomerProfile.SenderId },
        //        message = new { text = newSupportMess.Content }
        //    };

        //    var response = await client.PostAsJsonAsync(
        //        "https://openapi.zalo.me/v3.0/oa/message/cs",
        //        payload
        //    );
        //    var result = await response.Content.ReadAsStringAsync();
        //    if (!response.IsSuccessStatusCode)
        //        throw new Exception(result);
                
        //    // update staff message status pending -> send

        //    await UpdateSupportStaffMessageStatusSentAsync(newStaffSupportMes.Id);
        //}


        public async Task SendFacebookMesageAsync(CreateSupportStaffMessageRequest newSupportMess)
        {
            // create new support Staff mess
            var newStaffSupportMes = await CreateSupportStaffMessageAsync(newSupportMess);

            if (newStaffSupportMes == null)
            {
                throw new Exception("Create fail");
            }

            // get exist conversation

            var existConversation = await _supportConversationService.GetSupportConversationByIdAsync(newStaffSupportMes.SupportConversationId);

            if (existConversation == null)
            {
                throw new NotFoundException("No SupportConversation Found");
            }

            // get customer profile 

            var existCustomerProfile = await _customerProfileService.GetCustomerProfileByIdAsync(existConversation.ActiveCustomerId);

            if (existCustomerProfile == null)
            {
                throw new NotFoundException("No existCustomerProfile Found");
            }

            var pageAccessToken = _configuration["facebookWebHook:AccessToken"];

            if (string.IsNullOrEmpty(pageAccessToken))
                throw new Exception("Facebook Page Access Token is missing");

          

            using var httpClient = new HttpClient();

            var url =
                $"https://graph.facebook.com/v18.0/me/messages?access_token={pageAccessToken}";


            var body = new
            {
                recipient = new { id = existCustomerProfile.SenderId },
                message = new { text = newSupportMess.Content }
            };

            var response = await httpClient.PostAsJsonAsync(url, body);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Facebook Send API error: {error}");
            }
            // update staff message status pending -> send
            await UpdateSupportStaffMessageStatusSentAsync(newStaffSupportMes.Id);
        }

        public async Task SendInstagramMesageAsync(CreateSupportStaffMessageRequest newSupportMess)
        {
            // create new support Staff mess
            var newStaffSupportMes = await CreateSupportStaffMessageAsync(newSupportMess);

            if (newStaffSupportMes == null)
            {
                throw new Exception("Create fail");
            }

            // get exist conversation

            var existConversation = await _supportConversationService.GetSupportConversationByIdAsync(newStaffSupportMes.SupportConversationId);

            if (existConversation == null)
            {
                throw new NotFoundException("No SupportConversation Found");
            }

            // get customer profile 

            var existCustomerProfile = await _customerProfileService.GetCustomerProfileByIdAsync(existConversation.ActiveCustomerId);

            if (existCustomerProfile == null)
            {
                throw new NotFoundException("No existCustomerProfile Found");
            }

            var pageAccessToken = _configuration["InstagramWebhook:AccessToken"];

            if (string.IsNullOrEmpty(pageAccessToken))
                throw new Exception("Instagram Page Access Token is missing");

            var BussinessId = _configuration["InstagramWebhook:BusinessId"];
            if (string.IsNullOrEmpty(BussinessId))
                throw new Exception("Instagram Page Bussiness Id is missing");

            using var httpClient = new HttpClient();

            var url =
                $"https://graph.facebook.com/v19.0/me/messages?access_token={pageAccessToken}";


            var body = new
            {
                recipient = new { id = existCustomerProfile.SenderId},
                message = new { text = newSupportMess.Content }
            };

            var response = await httpClient.PostAsJsonAsync(url, body);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Facebook Send API error: {error}");
            }
            // update staff message status pending -> send
            await UpdateSupportStaffMessageStatusSentAsync(newStaffSupportMes.Id);
        }


        public async Task<CreateSupportStaffMessageResponse> CreateSupportStaffMessageAsync(CreateSupportStaffMessageRequest createSupportMessageRequest)
        {
            var newSupporttMessage = new SupportStaffMessage();

            var repo = _unitOfWork.GetRepository<SupportStaffMessage>();

             await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                // map request -> entity
                newSupporttMessage = _mapper.Map<SupportStaffMessage>(createSupportMessageRequest);

                // call repo save database

                await repo.InsertAsync(newSupporttMessage);

                // map entity -> response

          

            });
            return _mapper.Map<CreateSupportStaffMessageResponse>(newSupporttMessage);
        }


        public async Task UpdateSupportStaffMessageStatusSentAsync(Guid supportStaffMessageId)
        {
            var repo = _unitOfWork.GetRepository<SupportStaffMessage>();

            await _unitOfWork.ProcessInTransactionAsync(async () =>
           {
               // find exit supportStaffMessage
               var existingMessage = await repo.SingleOrDefaultAsync(predicate: ssm => ssm.Id == supportStaffMessageId);
               if (existingMessage == null)
               {
                   throw new NotFoundException("Support Staff Message does not exist");
               }
               existingMessage.Status = SupportStaffMessageStatus.Sended;

           });
        }


        public async Task<PagingResponse<GetAllSupportStaffMessageResponse>> GetAllSupportStaffMessageByStaffIdAsync(int pageNumber = 1, int pageSize = 20, Guid? staffId = null)
        {
            var repo = _unitOfWork.GetRepository<SupportStaffMessage>();

            return await repo.GetPagingListAsync(
               selector: x => new GetAllSupportStaffMessageResponse
               {
                   Id = x.Id,
                   Content = x.Content,
                   StaffId = x.StaffId,
                   Status = x.Status,
                   SupportConversationId = x.SupportConversationId,
                   Timestamp = x.Timestamp
               },
                 predicate: staffId == null
               ? null
               : x => x.StaffId == staffId.Value,
               orderBy: q => q.OrderByDescending(x => x.Timestamp),
               page: pageNumber,
               size: pageSize
               );
        }
    }
}
