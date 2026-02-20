using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.SignalRHub
{
    [Authorize]
    public class SupportConversationHub : Hub
    {
        private readonly ISupportStaffMessageService _supportStaffMessageService;

        public SupportConversationHub(ISupportStaffMessageService supportStaffMessageService)
        {
            _supportStaffMessageService = supportStaffMessageService;
        }

        public override async Task OnConnectedAsync()
        {
            var sub = Context.User?.FindFirst("sub")?.Value;
            var nameId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Console.WriteLine("=== SIGNALR CONNECTED ===");
            Console.WriteLine($"sub: {sub}");
            Console.WriteLine($"nameid: {nameId}");
            Console.WriteLine($"connectionId: {Context.ConnectionId}");

            await base.OnConnectedAsync();
        }

        // Join the conversation group to receive real-time updates for that conversation
        public async Task JoinConversationGroup(Guid conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation:{conversationId}");
        }

        // FE : await connection.invoke("JoinConversationGroup", conversationId);

        // Leave the conversation group when the user navigates away from the conversation
        public async Task LeaveConversationGroup(Guid conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation:{conversationId}");
        }

        // FE : await connection.invoke("LeaveConversationGroup", oldConversationId);

        // staff send message to conversation 

        public async Task StaffSendMessage(string providerName, CreateSupportStaffMessageRequest newStaffMessage)
        {
            if (providerName == "Zalo")
            {
                await _supportStaffMessageService.SendZaloMessageAsync(newStaffMessage);
            }
            else if (providerName == "Facebook")
            {
                await _supportStaffMessageService.SendFacebookMesageAsync(newStaffMessage);
            }
            else if (providerName == "Instagram")
            {
                await _supportStaffMessageService.SendInstagramMesageAsync(newStaffMessage);
            }
            else {
                throw new NotFoundException("No provider found");
            }
        }

        //FE : await connection.invoke("StaffSendMessage", providerName, newStaffMessage); after staff click send button
    }
}
