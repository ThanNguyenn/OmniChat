using OmniChat.Infrastructure.Dtos.Responses.WarningConversation;
using OmniChat.Infrastructure.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IConversationWarningService
    {
        public  Task<PagingResponse<WarningDetailRepsone>> GetAllWarningsAsync(int pageNumber = 1,int pageSize = 10,bool? isReviewed = null);

        public  Task<WarningDetailRepsone> GetWarningByIdAsync(Guid id);

        public  Task DeleteWarningAsync();
    }
}
