using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface ICustomerMergeService
    {
        Task<GetCustomerProfileResponse> MergeAndDeleteAsync(Guid sourceId, Guid targetId);

        public  Task HandleEnrichCustomerAsync(EnrichCustomerRequest dto);
    }
}
