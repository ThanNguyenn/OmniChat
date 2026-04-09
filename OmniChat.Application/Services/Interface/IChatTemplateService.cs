using OmniChat.Infrastructure.Dtos.Requests.ChatTemplate;
using OmniChat.Infrastructure.Dtos.Responses.ChatTemplate;
using OmniChat.Infrastructure.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IChatTemplateService
    {
        public Task<bool> CreateChatTemplateAsync(ChatTemplateRequest request);

        public Task<bool> UpdateChatTemplateAsync(Guid id, ChatTemplateRequest request);

        public Task<bool> DeleteChatTemplateAsync(Guid id);

        public Task<ChatTemplateResponse> GetChatTemplateByIdAsync(Guid id);

        public Task<PagingResponse<ChatTemplateResponse>> GetAllChatTemplateAsync(
         int pageNumber,
         int pageSize,
         string? search);

        public Task<string> ExpandTemplateCodesAsync(string message);
    }
}
