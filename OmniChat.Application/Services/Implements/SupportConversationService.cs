using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
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

        public async Task<SupportConversation> UpdateSupportConversationUpdateDateAsync(Guid Id)
        {
            
                // call repo
                var repo = _unitOfWork.GetRepository<SupportConversation>();

                // check exist 
                var existingSupportConversation = await GetSupportConversationByIdAsync(Id);
                if (existingSupportConversation == null)
                    throw new NotFoundException("No SupportConversation Found");

                existingSupportConversation.UpdateDate = DateTime.UtcNow;
                repo.Update(existingSupportConversation);
                return existingSupportConversation;
           
        }

        public async Task<PagingResponse<GetAllSupportConversationResponse>> SupportConversationByCustomerNamePagingAsync(int pageNumber = 1, int pageSize = 20, string? customerName = null)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();
            return await repo.GetPagingListAsync(
                selector: x => new GetAllSupportConversationResponse
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    Status = x.Status,
                    IsDistributed = x.IsDistributed,
                    CustomerName = x.CustomerName,
                    AvartarUrl = x.AvatarUrl,
                    ActiveStaffId = x.ActiveStaffId,
                    ActiveCustomerId = x.ActiveCustomerId,
                    ProvidersId = x.ProvidersId,
                },
                predicate: string.IsNullOrWhiteSpace(customerName)
                    ? null
                    : x => x.CustomerName.Contains(customerName),
                orderBy: q => q.OrderByDescending(x => x.CreatedDate),
                page: pageNumber,
                size: pageSize
            );
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
    }
}
