using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage;
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

        public async Task SendMessage(SendSupportMessageCommand command)
        {
            if(command.Provider == "Facebook")
            {
                await _supportStaffMessageService
              .SendFacebookMesageAsync(
                  new CreateSupportStaffMessageRequest
                  {
                      SupportConversationId = command.SupportConversationId,
                      StaffId = command.StaffId,
                      Content = command.Content
                  }
              );
            }
            else if (command.Provider == "Instagram")
            {
                await _supportStaffMessageService
                    .SendInstagramMesageAsync(
                        new CreateSupportStaffMessageRequest
                        {
                            SupportConversationId = command.SupportConversationId,
                            StaffId = command.StaffId,
                            Content = command.Content
                        }
                    );
            }
            else
            {
                throw new HubException("Unsupported provider");
            }
        }


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
