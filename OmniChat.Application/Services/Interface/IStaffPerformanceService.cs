using OmniChat.Infrastructure.Dtos.Responses.Performance;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IStaffPerformanceService
    {
        public  Task InitializePerformanceForAllStaffAsync();

        public  Task InitializePerformanceForStaffAsync(Guid staffId);

        public  Task UpdatePerformanceOnTaskCompleteAsync(Guid staffId, int handleTimeSeconds);

        public  Task UpdatePerformanceOnConversationCompleteAsync(Guid staffId, int firstResponseTimeSeconds);

        public  Task UpdatePerformanceOnTaskCancelAsync(Guid staffId, int handleTimeSeconds);

        public Task CompleteConversationAndTasksAsync(SupportConversation conversation);

        public  Task<TotalAverageResponse> GetTotalAverageAsync(DateTime fromDate, DateTime toDate);
    }
}
