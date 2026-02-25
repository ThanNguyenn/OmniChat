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

        public Task<SupportConversation> UpdateSupportConversationUpdateDateAsync(SupportConversation conversation);

        public Task<IEnumerable<StaffConversationSideBarResponse>> GetStaffConversationSideBarAsync(Guid staffId, string providerName);

        public Task<SupportConversationDetailResponse> GetConversationDetailByIdAsync(Guid conversationId);

        public  Task UpdateConversationAfterMergeAsync(CustomerProfile source, CustomerProfile target);

        public Task<SupportConversation> CreateNewSupportConversationAsync(CreateSupportConversationRequest request);
        public  Task<List<SupportConversationDetailResponse>> GetCustomerConversationHistoryAsync(Guid customerId);

        public Task<SupportConversation> GetSupportConversationHavePendingByCustomerIdAsync(Guid customerId, Guid providerId);

        public  Task<SupportConversation> AsignForSupportConversationByIdAsync(SupportConversation conversation, Guid staffAsignId);

        public Task<List<CompleteSupportConversationHistoryResponse>> GetCustomerCompleteSupportConversationHistoryAsync(Guid customerId);
    }
}
