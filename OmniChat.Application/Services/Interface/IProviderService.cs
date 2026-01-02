using OmniChat.Infrastructure.Dtos.Requests.Provider;
using OmniChat.Infrastructure.Dtos.Responses.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IProviderService
    {
        public  Task<CreateProviderResponse> CreateProviderAsync(CreateProviderRequest CreateProviderRequest);
    }
}
