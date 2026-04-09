using Amazon.Runtime;
using OmniChat.Infrastructure.Dtos.Requests.TaskAction;
using OmniChat.Infrastructure.Dtos.Responses.TaskAction;
using OmniChat.Infrastructure.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface ITaskActionService
    {
        public Task<PagingResponse<TaskActionResponse>> GetAllTaskActionAsync(int page, int pageSize);
        public Task<TaskActionResponse> GetTaskActionByIdAsync(Guid id);
        public Task<bool> CreateTaskActionAsync(TaskActionRequest actionRequest);
    }
}
