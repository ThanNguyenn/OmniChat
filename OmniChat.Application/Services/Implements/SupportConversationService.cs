using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using OmniChat.Application.Services.Interface;
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

        private readonly ICustomerProfileService _customerProfileService;

        private readonly IMessageKeywordFilterService _messageKeywordFilterService;
        public SupportConversationService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<SupportConversationService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, ICustomerProfileService customerProfileService, IMessageKeywordFilterService messageKeywordFilterService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _customerProfileService = customerProfileService;
            _messageKeywordFilterService = messageKeywordFilterService;
        }

        public async Task<SupportConversation> GetSupportConversationByIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            var exitSupportConversation = await repo.SingleOrDefaultAsync(predicate: sc => sc.Id == conversationId);

            if (exitSupportConversation == null)
                throw new NotFoundException("No supportConversation Found");

            return exitSupportConversation;
        }

        public async Task<SupportConversation> UpdateSupportConversationUpdateDateAsync(SupportConversation conversation)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            conversation.UpdateDate = DateTime.UtcNow;

                repo.Update(conversation);

            await _unitOfWork.CommitAsync();

            return conversation;
           
        }


        public async Task<SupportConversation> UpdateSupportConversationUpdateDateAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            var conversation = await repo.GetByIdAsync(conversationId);

            if (conversation == null)
                throw new Exception("Conversation not found");

            conversation.UpdateDate = DateTime.UtcNow;

            repo.Update(conversation);

            await _unitOfWork.CommitAsync();

            return conversation;
        }


        // Staff Pending SupportConversation side bar
        public async Task<IEnumerable<StaffConversationSideBarResponse>>GetStaffConversationSideBarAsync(Guid staffId, string providerName)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            var conversations = await repo.GetListAsync(
                predicate: c =>
                    c.ActiveStaffId == staffId &&
                    c.Status == ConversationStatus.Pending && (providerName == null ||
                c.Providers.ProviderName.ToLower() == providerName.ToLower()),

                orderBy: q => q.OrderByDescending(c => c.UpdateDate),

                selector: c => new StaffConversationSideBarResponse
                {
                    ConversationId = c.Id,
                    CustomerName = c.CustomerName,
                    AvartarUrl = c.AvatarUrl,
                    ProviderName = c.Providers.ProviderName,

                    LastMessage =
                        c.CustomerMessages
                            .Select(m => new { m.Content, m.Timestamp })
                        .Concat(
                            c.SupportStaffMessages
                                .Select(m => new { m.Content, m.Timestamp })
                        )
                        .OrderByDescending(m => m.Timestamp)
                        .Select(m => m.Content)
                        .FirstOrDefault() ?? string.Empty,

                    UnreadMessageCount = c.CustomerMessages.Count(m => m.IsRead == false)
                }
            );
            return conversations;
        }

        public async Task<SupportConversationDetailResponse>GetCustomerConversationHistoryAsync(Guid conversationId)
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
                throw new NotFoundException("Completed support conversation not found");

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


            var customerMessages = messages.Where(m => m.SenderType == "Customer");

            foreach (var message in customerMessages)
            {
                var result = await _messageKeywordFilterService.ExtractKeywords(message.Content);

                if (result.Highlights.Any() || result.Recommends.Any())
                    message.extractKeywordResponses = result;
            }

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
        public async Task<SupportConversationDetailResponse> GetConversationDetailByIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();
            var conversation = await repo.SingleOrDefaultAsync(predicate: sc => sc.Id == conversationId,
                include: source => source
                    .Include(c => c.CustomerMessages)
                    .Include(c => c.SupportStaffMessages)
            );


            if (conversation == null)
                throw new NotFoundException("No support conversation found");

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

            var customerMessages = messages.Where(m => m.SenderType == "Customer");

            foreach (var message in customerMessages)
            {
                var result = await _messageKeywordFilterService.ExtractKeywords(message.Content);

                if (result.Highlights.Any() || result.Recommends.Any())
                    message.extractKeywordResponses = result;
            }

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
                throw new BusinessException("Cannot assign completed conversation");

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

        public async Task<List<CompleteSupportConversationHistoryResponse>>GetCustomerCompleteSupportConversationHistoryAsync(Guid customerId)
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

        private async Task ReadAllCustomerMessageAsync(List<CustomerMessage> customerMessages)
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
    }
}
