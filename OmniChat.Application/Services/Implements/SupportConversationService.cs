using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Requests.SupportConversation;
using OmniChat.Infrastructure.Dtos.Responses.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class SupportConversationService : BaseService<SupportConversationService>, ISupportConversationService
    {
        private ISupportTaskService _supportTaskService;

        private readonly ICustomerProfileService _customerProfileService;

        private readonly ITaskAssignmentService _taskAssignmentService;

        private readonly INotificationService _notificationService;

        private readonly IHubContext<SidebarHub> _sidebarHubContext;

        public SupportConversationService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<SupportConversationService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, ICustomerProfileService customerProfileService, IHubContext<SidebarHub> sidebarHubContext, ISupportTaskService supportTaskService, INotificationService notificationService, ITaskAssignmentService taskAssignmentService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _customerProfileService = customerProfileService;
            _supportTaskService = supportTaskService;
            _supportTaskService = supportTaskService;
            _notificationService = notificationService;
            _taskAssignmentService = taskAssignmentService;
            _sidebarHubContext = sidebarHubContext;
        }

        public async Task<SupportConversation> GetSupportConversationByIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            var exitSupportConversation = await repo.SingleOrDefaultAsync(predicate: sc => sc.Id == conversationId,
                include: sc => sc
                    .Include(c => c.Staff)
                    .Include(c => c.Providers)
                    .Include(c => c.CustomerMessages)
                    .Include(c => c.SupportStaffMessages)
                    )
                ;

            if (exitSupportConversation == null)
                throw new NotFoundException("Không tìm thấy cuộc trò chuyện");

            return exitSupportConversation;
        }

        public async Task<PagingResponse<StaffConversationResponse>> GetStaffConversationAsync(
             Guid staffId,
             int pageNumber = 1,
             int pageSize = 20)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            var response = await repo.GetPagingListAsync<StaffConversationResponse>(
                predicate: sc => sc.ActiveStaffId == staffId && sc.Status == ConversationStatus.Pending,
                selector: sc => _mapper.Map<StaffConversationResponse>(sc),
                orderBy: q => q.OrderByDescending(sc => sc.CreatedDate),
                page: pageNumber,
                size: pageSize
            );

            return response;
        }

        public async Task<SupportConversation> UpdateSupportConversationUpdateDateAsync(SupportConversation conversation)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            conversation.UpdateDate = DateTime.UtcNow;

            repo.Update(conversation);

            await _unitOfWork.CommitAsync();

            return conversation;

        }

        public async Task<bool> CompleteConversationAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            var conversation = await GetSupportConversationByIdAsync(conversationId);

            if (conversation.Status == ConversationStatus.Complete)
            {
                throw new BadRequestException("Cuộc trò chuyện này đã được hoàn thành trước đó");
            }

            var conversationTasks = await _supportTaskService
                .GetSupportTaskByConversationIdAsync(conversationId);

            var allDone = conversationTasks.All(x => x.Status == SupportTaskStatus.Done);

            if (!allDone)
            {
                throw new BadRequestException("Chưa hoàn thành hết yêu cầu hỗ trợ");
            }

            conversation.Status = ConversationStatus.Complete;
            conversation.CloseAt = DateTime.UtcNow;
            conversation.UpdateDate = DateTime.UtcNow;
            repo.Update(conversation);
            await _unitOfWork.CommitAsync();

            await _taskAssignmentService.ProcessWaitingQueueAsync();

            if (conversation.ActiveStaffId.HasValue)
            {
                await PushSidebarToStaffAsync(conversation.ActiveStaffId.Value,conversation.Providers.ProviderName);
            }

            return true;
        }


        public async Task<List<SupportConversation>> GetConversationsForReminderAsync()
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();
            var conversations = await repo.GetQueryable()
                .Where(sc =>
                    sc.Status != ConversationStatus.Complete &&
                    sc.LastStaffMessageAt != null
                  ).Include(sc => sc.Staff)
                  .Include(sc => sc.Providers)
                .ToListAsync();
            return conversations;
        }

        public async Task UpdateConversationAsync(SupportConversation conversion)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            repo.Update(conversion);

            await _unitOfWork.CommitAsync();

        }


        public async Task<SupportConversation> UpdateSupportConversationUpdateDateAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            var conversation = await repo.GetByIdAsync(conversationId);

            if (conversation == null)
                throw new Exception("Không tìm thấy cuộc trò chuyện");

            conversation.UpdateDate = DateTime.UtcNow;

            repo.Update(conversation);

            await _unitOfWork.CommitAsync();

            return conversation;
        }


        // Staff Pending SupportConversation side bar
        public async Task<IEnumerable<StaffConversationSideBarResponse>> GetStaffConversationSideBarAsync(Guid staffId, string providerName)
        {
            Console.WriteLine($"[GetStaffConversationSideBarAsync] Join funtion");

            var repo = _unitOfWork.GetRepository<SupportConversation>();

            var conversations = await repo.GetListAsync(
                predicate: c =>
                    c.ActiveStaffId == staffId &&
                    (c.Status == ConversationStatus.Pending) &&
                    (string.IsNullOrEmpty(providerName) || c.Providers.ProviderName.ToLower() == providerName.ToLower()),

                orderBy: q => q.OrderByDescending(c => c.UpdateDate),

                selector: c => new StaffConversationSideBarResponse
                {
                    ConversationId = c.Id,
                    CustomerName = c.CustomerName,
                    AvartarUrl = c.AvatarUrl,
                    ProviderName = c.Providers.ProviderName,


                    LastMessage = c.CustomerMessages
                        .Select(m => new { m.Content, m.Timestamp })
                        .Concat(c.SupportStaffMessages.Select(m => new { m.Content, m.Timestamp }))
                        .OrderByDescending(m => m.Timestamp)
                        .Select(m => m.Content)
                        .FirstOrDefault() ?? string.Empty,


                    UpdateDate = c.CustomerMessages.Select(m => m.Timestamp)
                        .Concat(c.SupportStaffMessages.Select(m => m.Timestamp))
                        .OrderByDescending(t => t)
                        .FirstOrDefault(),

                    UnreadMessageCount = c.CustomerMessages.Count(m => m.IsRead == false)
                }
            );

            Console.WriteLine($"[GetStaffConversationSideBarAsync] conversations : {conversations}");
            if (conversations != null && conversations.Any())
            {
                var jsonLog = System.Text.Json.JsonSerializer.Serialize(conversations, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                Console.WriteLine($"[GetStaffConversationSideBarAsync] Đã tìm thấy {conversations.Count()} conversations:\n{jsonLog}");
            }
            else
            {
                Console.WriteLine("[GetStaffConversationSideBarAsync] Không có conversation nào thỏa mãn điều kiện.");
            }
            // ------------------------------------

            return conversations;
        }

        public async Task<SupportConversationDetailResponse> GetCustomerConversationHistoryAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            var conversation = await repo.GetQueryable()
                .AsNoTracking()
                .Where(sc =>
                    sc.Id == conversationId &&
                    sc.Status == ConversationStatus.Complete &&
                    sc.SupportTasks.Any(st => st.Status == SupportTaskStatus.Done)
                )
                .Include(sc => sc.CustomerMessages)
                .Include(sc => sc.SupportStaffMessages)
                .FirstOrDefaultAsync();

            if (conversation == null)
                throw new NotFoundException("Không tìm thấy lịch sử cuộc trò chuyện đã hoàn thành");

            var customerProfile = await _customerProfileService
                .GetCustomerProfileByIdAsync(conversation.ActiveCustomerId);

            var messages = conversation.CustomerMessages
                .Select(cm => new SupportConversationMessagesResponse
                {
                    SenderType = "Customer",
                    SenderId = customerProfile.Id,
                    Content = cm.Content,
                    Timestamp = cm.Timestamp
                })
                .Concat(
                    conversation.SupportStaffMessages.Select(sm =>
                        new SupportConversationMessagesResponse
                        {
                            SenderType = "Staff",
                            SenderId = sm.StaffId,
                            Content = sm.Content,
                            Timestamp = sm.Timestamp
                        })
                )
                .OrderBy(m => m.Timestamp)
                .ToList();

            return new SupportConversationDetailResponse
            {
                Id = conversation.Id,
                CreatedDate = conversation.CreatedDate ?? DateTime.UtcNow,
                Status = conversation.Status,
                IsDistributed = conversation.IsDistributed,
                CustomerName = conversation.CustomerName,
                AvartarUrl = conversation.AvatarUrl,
                ActiveStaffId = conversation.ActiveStaffId,
                ActiveCustomerId = conversation.ActiveCustomerId,
                ProvidersId = conversation.ProvidersId,
                Messages = messages
            };
        }

        // conversattion detail
        //public async Task<SupportConversationDetailResponse> GetConversationDetailByIdAsync(Guid conversationId)
        //{
        //    var repo = _unitOfWork.GetRepository<SupportConversation>();
        //    var conversation = await repo.SingleOrDefaultAsync(predicate: sc => sc.Id == conversationId,
        //        include: source => source
        //            .Include(c => c.CustomerMessages)
        //            .Include(c => c.Providers)
        //            .Include(c => c.SupportStaffMessages)
        //    );


        //    if (conversation == null)
        //        throw new NotFoundException("No support conversation found");

        //    await ReadAllCustomerMessageAsync(conversation.CustomerMessages.ToList());

        //    var customerProfile = await _customerProfileService.GetCustomerProfileByIdAsync(conversation.ActiveCustomerId);

        //    var messages = conversation.CustomerMessages.Select(cm => new SupportConversationMessagesResponse
        //    {
        //        SenderType = "Customer",
        //        SenderId = customerProfile.Id,
        //        Content = cm.Content,
        //        Timestamp = cm.Timestamp
        //    })
        //    .Concat(
        //        conversation.SupportStaffMessages.Select(sm => new SupportConversationMessagesResponse
        //        {
        //            SenderType = "Staff",
        //            SenderId = sm.StaffId,
        //            Content = sm.Content,
        //            Timestamp = sm.Timestamp
        //        })
        //        )
        //    .OrderBy(m => m.Timestamp)
        //    .ToList();

        //    var recentCustomerMessages = messages
        //.Where(m => m.SenderType == "Customer")
        //.OrderByDescending(m => m.Timestamp)
        //.Take(5)
        //.ToList();

        //    var customerMessages = messages.Where(m => m.SenderType == "Customer");

        //    foreach (var message in recentCustomerMessages)
        //    {
        //        if (!string.IsNullOrEmpty(message.Content))
        //        {
        //            var result = await _messageKeywordFilterService.ExtractKeywords(message.Content);

        //            if (result.Highlights.Any() || result.Recommends.Any())
        //            {
        //                message.extractKeywordResponses = result;
        //            }
        //        }
        //    }


        //    var lastMessage = conversation.CustomerMessages
        //    .OrderByDescending(m => m.Timestamp)
        //    .FirstOrDefault();

        //    var sidebarUpdate = new StaffConversationSideBarUpdateResponse
        //    {
        //        ConversationId = conversation.Id,
        //        CustomerName = conversation.CustomerName,
        //        avartarUrl = conversation.AvatarUrl,
        //        providerName = conversation.Providers.ProviderName,
        //        LastMessage = lastMessage.Content,
        //        UnreadMessageCount = 0,
        //    };

        //    await _hubContext.Clients
        //        .User(conversation.ActiveStaffId.ToString())
        //        .SendAsync("SidebarUpdated", sidebarUpdate);

        //    await _notificationService.UpdateNotificationIsReadAsync(conversationId);

        //    return new SupportConversationDetailResponse
        //    {
        //        Id = conversation.Id,
        //        CreatedDate = conversation.CreatedDate ?? DateTime.UtcNow,
        //        Status = conversation.Status,
        //        IsDistributed = conversation.IsDistributed,
        //        CustomerName = conversation.CustomerName,
        //        AvartarUrl = conversation.AvatarUrl,
        //        ActiveStaffId = conversation.ActiveStaffId,
        //        ActiveCustomerId = conversation.ActiveCustomerId,
        //        ProvidersId = conversation.ProvidersId,

        //        Messages = messages
        //    };
        //}

        public async Task<SupportConversationDetailResponse> GetConversationDetailByIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();
            var conversation = await repo.SingleOrDefaultAsync(predicate: sc => sc.Id == conversationId,
                include: source => source
                    .Include(c => c.CustomerMessages)
                    .Include(c => c.Providers)
                    .Include(c => c.SupportStaffMessages)
            );

            if (conversation == null)
                throw new NotFoundException("Không tìm thấy cuộc trò chuyện");


            await ReadAllCustomerMessageAsync(conversation.CustomerMessages.ToList());

            var customerProfile = await _customerProfileService.GetCustomerProfileByIdAsync(conversation.ActiveCustomerId);


            var messages = conversation.CustomerMessages.Select(cm => new SupportConversationMessagesResponse
            {
                SenderType = "Customer",
                SenderId = customerProfile.Id,
                Content = cm.Content,
                Timestamp = cm.Timestamp
            })
            .Concat(
                conversation.SupportStaffMessages.Select(sm => new SupportConversationMessagesResponse
                {
                    SenderType = "Staff",
                    SenderId = sm.StaffId,
                    Content = sm.Content,
                    Timestamp = sm.Timestamp
                })
            )
            .OrderBy(m => m.Timestamp)
            .ToList();

            await PushSidebarToStaffAsync(conversation.ActiveStaffId.Value,conversation.Providers.ProviderName);

            await _notificationService.UpdateNotificationIsReadAsync(conversationId);

            return new SupportConversationDetailResponse
            {
                Id = conversation.Id,
                CreatedDate = conversation.CreatedDate ?? DateTime.UtcNow,
                Status = conversation.Status,
                IsDistributed = conversation.IsDistributed,
                CustomerName = conversation.CustomerName,
                AvartarUrl = conversation.AvatarUrl,
                ActiveStaffId = conversation.ActiveStaffId,
                ActiveCustomerId = conversation.ActiveCustomerId,
                ProvidersId = conversation.ProvidersId,
                Messages = messages
            };
        }

        public async Task UpdateConversationAfterMergeAsync(CustomerProfile source, CustomerProfile target)
        {
            var conversationRepo = _unitOfWork.GetRepository<SupportConversation>();

            var conversations = await conversationRepo
                .GetQueryable()
                .Where(x => x.ActiveCustomerId == source.Id)
                .ToListAsync();

            foreach (var conv in conversations)
            {
                conv.ActiveCustomerId = target.Id;
                conversationRepo.Update(conv);
            }
        }

        public async Task<SupportConversation> CreateNewSupportConversationAsync(CreateSupportConversationRequest request)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            var entity = _mapper.Map<SupportConversation>(request);

            await repo.InsertAsync(entity);
            await _unitOfWork.CommitAsync();

            return entity; // trả entity luôn
        }

        public async Task<SupportConversation> GetSupportConversationHavePendingByCustomerIdAsync(Guid customerId, Guid providerId)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            return await repo.SingleOrDefaultAsync(predicate:
                epc => epc.ActiveCustomerId == customerId
                    && epc.ProvidersId == providerId
                    && epc.Status == ConversationStatus.Pending);
        }

        public async Task<SupportConversation> AsignForSupportConversationByIdAsync(SupportConversation conversation, Guid staffAsignId)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            if (conversation.Status == ConversationStatus.Complete)
                throw new BusinessException("Không thể phân công cho cuộc trò chuyện đã hoàn thành");

            if (conversation.IsDistributed)
            {
                return conversation;
            }

            if (conversation.ActiveStaffId == null)
            {
                conversation.ActiveStaffId = staffAsignId;
            }

            conversation.IsDistributed = true;

            repo.Update(conversation);

            await _unitOfWork.CommitAsync();

            return conversation;
        }

        public async Task<List<CompleteSupportConversationHistoryResponse>> GetCustomerCompleteSupportConversationHistoryAsync(Guid customerId)
        {
            return await _unitOfWork.GetRepository<SupportConversation>()
                .GetQueryable()
                .AsNoTracking()
                .Where(sc => sc.ActiveCustomerId == customerId &&
                             sc.Status == ConversationStatus.Complete)
                .SelectMany(sc => sc.SupportTasks
                    .Where(st => st.Status == SupportTaskStatus.Done)
                    .Select(st => new CompleteSupportConversationHistoryResponse
                    {
                        Id = sc.Id,
                        ProviderName = sc.Providers.ProviderName,
                        Status = sc.Status,
                        CompleteDate = st.CompleteDate ?? DateTime.UtcNow,
                        KeywordType = st.IntentType != null
                                        ? st.IntentType.TypeName
                                        : null,
                        StaffName = st.CurrentAssignedStaff != null
                                        ? st.CurrentAssignedStaff.Name
                                        : null
                    }))
                .ToListAsync();
        }

        public async Task ReadAllCustomerMessageAsync(List<CustomerMessage> customerMessages)
        {
            var repo = _unitOfWork.GetRepository<CustomerMessage>();

            foreach (var message in customerMessages)
            {
                if (message.IsRead == false)
                {
                    message.IsRead = true;
                }
            }

            repo.UpdateRange(customerMessages);

            await _unitOfWork.CommitAsync();
        }

        public async Task PushSidebarToStaffAsync(Guid staffId, string providerName = "")
        {
            try
            {
                var conversations = await GetStaffConversationSideBarAsync(staffId, providerName);

                await _sidebarHubContext.Clients.User(staffId.ToString()).SendAsync("SidebarUpdated", conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pushing sidebar update to staff {StaffId} for provider {ProviderName}", staffId, providerName);
            }
        }
    }
}
