using OmniChat.Infrastructure.Dtos.Requests.FeedBack;
using OmniChat.Infrastructure.Dtos.Responses.FeedBack;
using OmniChat.Infrastructure.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IFeedBackService
    {
        public Task<PagingResponse<FeedBackResponse>> GetFeedBackByStaffIdAsync(
      Guid staffId,
      int pageIndex = 1,
      int pageSize = 10);

        public Task<FeedBackResponse> GetFeedBackByIdAsync(Guid feedBackId);

        public  Task<bool> ErichFeedBackFormAsync(Guid conversationId,FeedBackRequest feedBackRequest,string formUrl);
    }
}
