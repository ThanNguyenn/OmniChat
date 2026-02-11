using OmniChat.Infrastructure.Dtos.Requests.SupportConversation;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface ISupportConversationService
    {

        public Task<SupportConversation> GetSupportConversationByIdAsync(Guid conversationId);

        public Task<SupportConversation> UpdateSupportConversationUpdateDateAsync(Guid Id);

        public Task<IEnumerable<StaffConversationSideBarResponse>> GetStaffConversationSideBarAsync(Guid staffId, string providerName);

        public Task<SupportConversationDetailResponse> GetConversationDetailByIdAsync(Guid conversationId);

        public  Task UpdateConversationAfterMergeAsync(CustomerProfile source, CustomerProfile target);

        public  Task<SupportConversationDetailResponse> CreateNewSupportConversationAsync(CreateSupportConversationRequest request);

        public  Task<List<SupportConversationDetailResponse>> GetCustomerConversationHistoryAsync(Guid customerId);
    }
}
