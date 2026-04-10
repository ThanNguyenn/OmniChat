using OmniChat.Infrastructure.Dtos.Requests.TaskCancelReason;
using OmniChat.Infrastructure.Dtos.Responses.SupportTask;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface ISupportTaskService
    {
        public  Task<IEnumerable<SupportTask>> GetDoneSupportTaskByConversationIdAsync(Guid conversationId);

        public  Task<IEnumerable<SupportTask>> GetSupportTaskByConversationIdAsync(Guid conversationId);

        public  Task<IEnumerable<SupportTasksResponse>> GetSupportTaskOnConversationIdAsync(Guid conversationId);

        public  Task<bool> CompleteTaskAsync(Guid taskId);

        Task<IEnumerable<DashboardMonthResponse>> GetTaskIntentDashboardResponsesAsync(string year);
        public  Task<bool> CancelSupportTaskAsync(Guid taskId, TaskCancelReasonRequest cancelReasonRequest);

    }
}
