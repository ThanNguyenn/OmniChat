using Microsoft.AspNetCore.SignalR;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.SignalRHub
{
    public class SupportConversationHub : Hub
    {
        private readonly ISupportStaffMessageService _supportStaffMessageService;

        public SupportConversationHub(ISupportStaffMessageService supportStaffMessageService)
        {
            _supportStaffMessageService = supportStaffMessageService;
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
        public async Task JoinStaffGroup(string staffId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"staff:{staffId}"
            );
        }

        public async Task LeaveStaffGroup(string staffId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"staff:{staffId}"
            );
        }
    }
}
