using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Application.Webhooks.Zalo.WebhookMessage;
using OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage;
using OmniChat.Infrastructure.Dtos.Responses.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
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
        private readonly IProviderService _providerService;
        private readonly IHubContext<SupportConversationHub> _hubContext;

        public SupportStaffMessageService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<SupportStaffMessageService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IZaloOAuthService zaloOAuthService, ICustomerProfileService customerProfileService, ISupportConversationService supportConversationService, IConfiguration configuration,IHubContext<SupportConversationHub> hubContext, IProviderService providerService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _httpClient = httpClient;
            _zaloOAuthService = zaloOAuthService;
            _customerProfileService = customerProfileService;
            _supportConversationService = supportConversationService;
            _configuration = configuration;
            _hubContext = hubContext;
            _providerService = providerService;
        }

        public async Task<bool> SendZaloMessageAsync(CreateSupportStaffMessageRequest newSupportMess)
        {
            _logger.LogInformation(
               "[ZALO-SEND] Start sending message | ConversationId={ConversationId} | StaffId={StaffId}",
               newSupportMess.SupportConversationId,
               newSupportMess.StaffId
            );
            // create new support Staff mess
            var newStaffSupportMes = await CreateSupportStaffMessageAsync(newSupportMess);

            if (newStaffSupportMes == null)
            {
                _logger.LogError(
                       "[ZALO-SEND] Failed to create SupportStaffMessage | ConversationId={ConversationId}",
                       newSupportMess.SupportConversationId
                   );
                throw new Exception("Create fail");
            }

            // get exist conversation

            var existConversation = await _supportConversationService.GetSupportConversationByIdAsync(newStaffSupportMes.SupportConversationId);

            if (existConversation == null)
            {
                _logger.LogError(
                    "[ZALO-SEND] Conversation not found | ConversationId={ConversationId}",
                    newStaffSupportMes.SupportConversationId
                );
                throw new NotFoundException("No SupportConversation Found");
            }

            // get customer profile 

            var existCustomerProfile = await _customerProfileService.GetCustomerProfileByIdAsync(existConversation.ActiveCustomerId);

            if (existCustomerProfile == null)
            {
                        _logger.LogError(
                    "[ZALO-SEND] CustomerProfile not found | CustomerId={CustomerId}",
                    existConversation.ActiveCustomerId
                );
                throw new NotFoundException("No existCustomerProfile Found");
            }
            _logger.LogInformation(
                "[ZALO-SEND] Sending to Zalo | ZaloUserId={ZaloUserId} | ConversationId={ConversationId}",
                existCustomerProfile.ZaloSenderId,
                existConversation.Id
            );

            var accessToken = await _zaloOAuthService.GetAccessTokenAsync();

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("access_token", accessToken);

            var payload = new
            {
                recipient = new ZaloRecipient { UserId = long.Parse(existCustomerProfile.ZaloSenderId) },
                message = new { text = newSupportMess.Content }
            };

            _logger.LogInformation(
                   "[ZALO-SEND] Payload prepared | ConversationId={ConversationId}",
                   existConversation.Id
               );

            var response = await client.PostAsJsonAsync(
                "https://openapi.zalo.me/v3.0/oa/message/cs",
                payload
            );
            var result = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
               "[ZALO-SEND] Zalo response | StatusCode={StatusCode} | Body={Body} | ConversationId={ConversationId}",
               response.StatusCode,
               result,
               existConversation.Id
            );

            if (!response.IsSuccessStatusCode) {

                _logger.LogError(
                     "[ZALO-SEND] Failed sending to Zalo | StatusCode={StatusCode} | Body={Body}",
                     response.StatusCode,
                     result
                 );

                throw new Exception(result);
            }

            var provider = await _providerService.GetProviderByIdAsync(existConversation.ProvidersId);


            // update staff message status pending -> send

            newStaffSupportMes.Status = SupportStaffMessageStatus.Sent;

            _logger.LogInformation(
                "[ZALO-SEND] Message sent successfully | MessageId={MessageId}",
                newStaffSupportMes.Id
            );

            existConversation.UpdateDate = DateTime.UtcNow;
            _unitOfWork.GetRepository<SupportConversation>().Update(existConversation);

            // Update Sidebar for Staff via SignalR (GROUP-BASED)

            var sidebarUpdate = new StaffConversationSideBarUpdateResponse
            {
                ConversationId = existConversation.Id,
                CustomerName = existConversation.CustomerName,
                avartarUrl = existConversation.AvatarUrl,
                providerName = provider.ProviderName,
                LastMessage = newStaffSupportMes.Content
            };
             await _hubContext.Clients
                .User(existConversation.ActiveStaffId.ToString())
                .SendAsync("SidebarUpdated", sidebarUpdate);


            // Update conversationDetail for Staff via SignalR

            var supportConversationMessages = new SupportConversationMessagesResponse
            {
                SenderType = "Staff",
                SenderId = newStaffSupportMes.StaffId,
                Content = newStaffSupportMes.Content,
                Timestamp = newStaffSupportMes.Timestamp
            };
                
            await _hubContext.Clients.Group($"conversation:{existConversation.Id}")
                .SendAsync("StaffReceiveMessage", supportConversationMessages);

            return true;
        }


        public async Task<bool> SendFacebookMesageAsync(CreateSupportStaffMessageRequest newSupportMess)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                // create new support Staff mess
                var newStaffSupportMes = await CreateSupportStaffMessageAsync(newSupportMess);

                if (newStaffSupportMes == null)
                {
                    throw new ValidationException("Create fail");
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
                    throw new BusinessException("Facebook Page Access Token is missing");



                using var httpClient = new HttpClient();

                var url =
                    $"https://graph.facebook.com/v18.0/me/messages?access_token={pageAccessToken}";

                var body = new
                {
                    recipient = new { id = existCustomerProfile.FacebookSenderId },
                    message = new { text = newSupportMess.Content }
                };

                var response = await httpClient.PostAsJsonAsync(url, body);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new BusinessException($"Facebook Send API error: {error}");
                }

                newStaffSupportMes.Status = SupportStaffMessageStatus.Sent;
                // After create new staff message Update Supportconversation UpdateDate -> now

                //var conversation = await _supportConversationService.UpdateSupportConversationUpdateDateAsync(newSupportMess.SupportConversationId);
                existConversation.UpdateDate = DateTime.UtcNow;
                _unitOfWork.GetRepository<SupportConversation>().Update(existConversation);

                var provider = await _providerService.GetProviderByIdAsync(existConversation.ProvidersId);

                // Update Sidebar for Staff via SignalR (GROUP-BASED)

                var sidebarUpdate = new StaffConversationSideBarUpdateResponse
                {
                    ConversationId = existConversation.Id,
                    CustomerName = existConversation.CustomerName,
                    avartarUrl = existConversation.AvatarUrl,
                    providerName = provider.ProviderName,
                    LastMessage = newStaffSupportMes.Content
                };
                await _hubContext.Clients
                   .User(existConversation.ActiveStaffId.ToString())
                   .SendAsync("SidebarUpdated", sidebarUpdate);


                // Update conversationDetail for Staff via SignalR

                var supportConversationMessages = new SupportConversationMessagesResponse
                {
                    SenderType = "Staff",
                    SenderId = newStaffSupportMes.StaffId,
                    Content = newStaffSupportMes.Content,
                    Timestamp = newStaffSupportMes.Timestamp
                };

                await _hubContext.Clients.Group($"conversation:{existConversation.Id}")
                    .SendAsync("StaffReceiveMessage", supportConversationMessages);

                return true;
            });
        }

        public async Task<bool> SendInstagramMesageAsync(CreateSupportStaffMessageRequest newSupportMess)
        {

            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                // create new support Staff mess
                var newStaffSupportMes = await CreateSupportStaffMessageAsync(newSupportMess);

                if (newStaffSupportMes == null)
                {
                    throw new ValidationException("Create fail");
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

                var pageAccessToken = _configuration["InstagramWebhook:InstagramPageAccessToken"];

                if (string.IsNullOrEmpty(pageAccessToken))
                    throw new BusinessException("Instagram Page Access Token is missing");

                var BussinessId = _configuration["InstagramWebhook:BusinessId"];
                if (string.IsNullOrEmpty(BussinessId))
                    throw new BusinessException("Instagram Page Bussiness Id is missing");

                using var httpClient = new HttpClient();

                var url =
                    $"https://graph.facebook.com/v19.0/me/messages?access_token={pageAccessToken}";


                var body = new
                {
                    recipient = new { id = existCustomerProfile.InstagramSenderId },
                    message = new { text = newSupportMess.Content }
                };

                var response = await httpClient.PostAsJsonAsync(url, body);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new BusinessException($"Instagram Send API error: {error}");

                }

                newStaffSupportMes.Status = SupportStaffMessageStatus.Sent;
                // After create new staff message Update Supportconversation UpdateDate -> now

                // var conversation = await _supportConversationService.UpdateSupportConversationUpdateDateAsync(newSupportMess.SupportConversationId);

                existConversation.UpdateDate = DateTime.UtcNow;
                _unitOfWork.GetRepository<SupportConversation>().Update(existConversation);

                var provider = await _providerService.GetProviderByIdAsync(existConversation.ProvidersId);
               
                // Update Sidebar for Staff via SignalR (GROUP-BASED)

                var sidebarUpdate = new StaffConversationSideBarUpdateResponse
                {
                    ConversationId = existConversation.Id,
                    CustomerName = existConversation.CustomerName,
                    avartarUrl = existConversation.AvatarUrl,
                    providerName = provider.ProviderName,
                    LastMessage = newStaffSupportMes.Content
                };
                await _hubContext.Clients
                   .User(existConversation.ActiveStaffId.ToString())
                   .SendAsync("SidebarUpdated", sidebarUpdate);


                // Update conversationDetail for Staff via SignalR

                var supportConversationMessages = new SupportConversationMessagesResponse
                {
                    SenderType = "Staff",
                    SenderId = newStaffSupportMes.StaffId,
                    Content = newStaffSupportMes.Content,
                    Timestamp = newStaffSupportMes.Timestamp
                };

                await _hubContext.Clients.Group($"conversation:{existConversation.Id}")
                    .SendAsync("StaffReceiveMessage", supportConversationMessages);
                return true;
            });
        }


        public async Task<SupportStaffMessage> CreateSupportStaffMessageAsync(CreateSupportStaffMessageRequest createSupportMessageRequest)
        {
            var repo = _unitOfWork.GetRepository<SupportStaffMessage>();

            var entity = _mapper.Map<SupportStaffMessage>(createSupportMessageRequest);

            await repo.InsertAsync(entity);

            return entity;
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
