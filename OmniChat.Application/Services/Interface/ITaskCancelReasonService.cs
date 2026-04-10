using OmniChat.Infrastructure.Dtos.Requests.TaskCancelReason;
using OmniChat.Infrastructure.Dtos.Responses.TaskCancelReason;
using OmniChat.Infrastructure.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface ITaskCancelReasonService
    {
        public  Task<PagingResponse<TaskCancelReasonResponse>> GetAllTaskCancelReasonAsync(int page = 1, int pageSize = 10);

        public  Task<TaskCancelReasonResponse> GetTaskCancelReasonBySupportTaskIdAsync(Guid supportTaskId);

    }
}
