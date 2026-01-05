using OmniChat.Infrastructure.Dtos.Requests.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Responses.CustomerMessage;
using OmniChat.Infrastructure.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface ICustomerMessageService
    {

        public Task<CreateCustomerMessageResponse> CreateCustomerMessageAsync(CreateCustomerMessageRequest createCustomerMessageRequest);

        public  Task<PagingResponse<GetAllCustomerMessageResponse>> GetAllCustomerMessageByCustomerIdAsync(int pageNumber = 1, int pageSize = 20, Guid? customerId = null);


    }
}
