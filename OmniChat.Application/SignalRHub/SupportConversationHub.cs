using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.SignalRHub
{
    public class SupportConversationHub : Hub
    {
        public async Task JoinConversation(Guid conversationId)
        {
            // show the realtime message on the current conversation was chosen on the conversation detail
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"conversation:{conversationId}"
            );
        }

        // leave conversation, No show the old message on the new conversation 
        public async Task LeaveConversation(Guid conversationId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"conversation:{conversationId}"
            );
        }
    }
}
