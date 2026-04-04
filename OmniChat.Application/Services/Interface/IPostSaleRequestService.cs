using OmniChat.Infrastructure.Dtos.Requests.PostSaleRequest;
using OmniChat.Infrastructure.Dtos.Responses.PostSaleRequest;
using OmniChat.Infrastructure.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IPostSaleRequestService
{
    Task<PagingResponse<GetPostSaleRequestsResponse>> GetPostSaleRequestsAsync(int pageNumber = 1, int pageSize = 20, string sortBy = "createdDate", bool descending = true);

    Task<GetPostSaleRequestByIdResponse> GetPostSaleRequestByIdAsync(Guid id);

    Task<bool> CreatePostSaleRequestAsync(CreatePostSaleRequestRequest request);

    Task<bool> UpdatePostSaleRequestAsync(Guid id, UpdatePostSaleRequestRequest request);

    Task<bool> DeletePostSaleRequestAsync(Guid id);

    Task<bool> AcceptPostSaleRequestAsync(Guid id);

    Task<bool> RejectPostSaleRequestAsync(Guid id);
}
