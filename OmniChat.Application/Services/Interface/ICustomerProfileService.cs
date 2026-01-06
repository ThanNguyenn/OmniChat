using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface ICustomerProfileService
    {

        public  Task<PagingResponse<GetCustomerProfileResponse>> GetCustomerProfilesPagingAsync(int pageNumber = 1, int pageSize = 20, string? customerName = null);

        public Task<CustomerProfile> GetCustomerProfileByIdAsync(Guid customerProfileId);

        public  Task<CustomerProfile> GetCustomerProfileBySenderAndProviderIdIdAsync(long senderId,Guid providersId);

        public  Task<CustomerProfile> CreateCustomerProfileEntityAsync(CreateCustomerProfileRequest request);
    }
}
