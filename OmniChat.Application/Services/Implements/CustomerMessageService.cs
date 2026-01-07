using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Responses.CustomerMessage;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class CustomerMessageService : BaseService<CustomerMessageService> , ICustomerMessageService
    {
        public CustomerMessageService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<CustomerMessageService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<CreateCustomerMessageResponse> CreateCustomerMessageAsync(CreateCustomerMessageRequest createCustomerMessageRequest)
        {
           
            
                return await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    // call repo 
                    var repo = _unitOfWork.GetRepository<CustomerMessage>();

                    // Map request => Entity

                    var entity = _mapper.Map<CustomerMessage>(createCustomerMessageRequest);

                    // Insert Database

                    await   repo.InsertAsync(entity);

                    // Map entity =>  response

                    return _mapper.Map<CreateCustomerMessageResponse>(entity);

                });
        }

        public async Task<PagingResponse<GetAllCustomerMessageResponse>> GetAllCustomerMessageByCustomerIdAsync(int pageNumber = 1, int pageSize = 20, Guid? customerId = null)
        {
            
                var repo = _unitOfWork.GetRepository<CustomerMessage>();

            return await repo.GetPagingListAsync(
            selector: x => new GetAllCustomerMessageResponse
            {
                Id = x.Id,
                Content = x.Content,
                Timestamp = x.Timestamp,
                KeywordActive = x.KeywordActive,
                CustomerId = x.CustomerId,
                ConversationId = x.ConversationId
            },
              predicate: customerId == null
            ? null
            : x => x.CustomerId == customerId.Value,
        orderBy: q => q.OrderByDescending(x => x.Timestamp),
        page: pageNumber,
        size: pageSize
            );
        }
    }
}
