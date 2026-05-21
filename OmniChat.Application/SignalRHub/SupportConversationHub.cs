using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
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
        private readonly ICustomerMessageService _customerMessageService;
        private readonly ISupportConversationService _supportConversationService;
        public SupportConversationHub(ISupportStaffMessageService supportStaffMessageService, ICustomerMessageService customerMessageService, ISupportConversationService supportConversationService)
        {
            _supportStaffMessageService = supportStaffMessageService;
            _customerMessageService = customerMessageService;
            _supportConversationService = supportConversationService;
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

        // Join the conversation group to receive real-time updates for that conversation and update unread message count in sidebar
        public async Task JoinConversationGroup(Guid conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation:{conversationId}");

            await _customerMessageService.MarkAsReadByConversationIdAsync(conversationId);

            var conversation = await _supportConversationService.GetSupportConversationByIdAsync(conversationId);
            var userId = Context.UserIdentifier;

            if (Guid.TryParse(userId, out var staffId))
            {

                await _supportConversationService.PushSidebarToStaffAsync(staffId, conversation.Providers.ProviderName);
            }

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
            try
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
                else
                {
                    throw new Exception("Provider not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR StaffSendMessage: " + ex.ToString());
                throw;
            }
        }

        //FE : await connection.invoke("StaffSendMessage", providerName, newStaffMessage); after staff click send button
    }
}
