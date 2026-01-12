using OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage;
using OmniChat.Infrastructure.Dtos.Responses.SupportStaffMessage;
using OmniChat.Infrastructure.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface ISupportStaffMessageService
    {
        public  Task<CreateSupportStaffMessageResponse> CreateSupportStaffMessageAsync(CreateSupportStaffMessageRequest createSupportMessageRequest);

        public  Task UpdateSupportStaffMessageStatusSentAsync(Guid supportStaffMessageId);

        public  Task<PagingResponse<GetAllSupportStaffMessageResponse>> GetAllSupportStaffMessageByStaffIdAsync(int pageNumber = 1, int pageSize = 20, Guid? staffId = null);

        //public  Task SendZaloMessageAsync(CreateSupportStaffMessageRequest newSupportMess);

        public  Task<bool> SendFacebookMesageAsync(CreateSupportStaffMessageRequest newSupportMess);

        public  Task<bool> SendInstagramMesageAsync(CreateSupportStaffMessageRequest newSupportMess);
    }
}
