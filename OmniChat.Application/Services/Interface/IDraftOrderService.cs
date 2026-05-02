using OmniChat.Infrastructure.Dtos.Requests.Order;
using OmniChat.Infrastructure.Dtos.Responses.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IDraftOrderService
{
    Task<bool> CreateDraftOrderAsync(Guid customerId, string message);
    Task<List<DraftOrderItem>> PreviewDraftOrderAsync(Guid customerId, List<string> messages);
    Task<bool> CreateDraftOrderFromConversationAsync(Guid conversationId);
}
