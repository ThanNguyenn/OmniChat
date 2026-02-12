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
        public SupportConversationService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<SupportConversationService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, ICustomerProfileService customerProfileService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _customerProfileService = customerProfileService;
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
                        .FirstOrDefault() ?? string.Empty
                }
            );
            return conversations;
        }

        public async Task<List<SupportConversationDetailResponse>> GetCustomerConversationHistoryAsync(Guid customerId)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            var conversations = await repo.GetListAsync(predicate: sc => sc.ActiveCustomerId == customerId,

                include: source => source
                .Include(c => c.CustomerMessages)
                .Include(c => c.SupportStaffMessages)
                );

            if (conversations == null)
                throw new NotFoundException("No support conversation found");

      
            var result = conversations.Select( conversation =>
            {
                // sum all message 
                var allMessages = new List<SupportConversationMessagesResponse>();

                if (conversation.CustomerMessages != null)
                {
                    allMessages.AddRange(conversation.CustomerMessages.Select(m =>
                    new SupportConversationMessagesResponse
                    {
                        SenderType = "Customer",
                        SenderId = m.CustomerId,
                        Content = m.Content,
                        Timestamp = m.Timestamp
                    }
                    ));
                }

                if (conversation.SupportStaffMessages != null)
                {
                    allMessages.AddRange(conversation.SupportStaffMessages.Select(m =>
                        new SupportConversationMessagesResponse
                        {
                            SenderType = "Staff",
                            SenderId = m.StaffId,
                            Content = m.Content,
                            Timestamp = m.Timestamp
                        }));
                }
                allMessages = allMessages
                 .OrderBy(m => m.Timestamp)
                 .ToList();

                return new SupportConversationDetailResponse
                {
                    Id = conversation.Id,
                    CreatedDate = conversation.CreatedDate,
                    Status = conversation.Status,
                    IsDistributed = conversation.IsDistributed,
                    CustomerName = conversation.CustomerName,
                    AvartarUrl = conversation.AvatarUrl,
                    ActiveStaffId = conversation.ActiveStaffId,
                    ActiveCustomerId = conversation.ActiveCustomerId,
                    ProvidersId = conversation.ProvidersId,
                    Messages = allMessages
                };

            })
            .OrderByDescending(c => c.Messages.LastOrDefault()?.Timestamp ?? 0)
            .ToList();

            return result;
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

            return new SupportConversationDetailResponse
            {
                Id = conversation.Id,
                CreatedDate = conversation.CreatedDate,
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

           if(conversation.ActiveStaffId == null)
            {
                // Assign staff when no have staff support
                conversation.ActiveStaffId = staffAsignId;
            }

            repo.Update(conversation);

            await _unitOfWork.CommitAsync();

            return conversation;
        }
    }
}
