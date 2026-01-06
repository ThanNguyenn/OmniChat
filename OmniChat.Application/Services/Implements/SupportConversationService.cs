using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
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
        public SupportConversationService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<SupportConversationService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<SupportConversation> GetSupportConversationByIdAsync(Guid conversationId)
        {
            var repo = _unitOfWork.GetRepository<SupportConversation>();

            var exitSupportConversation = await repo.SingleOrDefaultAsync(predicate: sc => sc.Id == conversationId);

            if (exitSupportConversation == null)
                throw new NotFoundException("No supportConversation Found");

            return exitSupportConversation;
        }

        public async Task<PagingResponse<GetAllSupportConversationResponse>> SupportConversationByCustomerNamePagingAsync(int pageNumber = 1,int pageSize = 20,string? customerName = null)
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
                    AvartarUrl = x.AvartarUrl,
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
    }
}
