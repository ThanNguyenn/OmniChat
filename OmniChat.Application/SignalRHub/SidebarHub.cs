using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OmniChat.Application.Services.Interface;
using Microsoft.AspNetCore.Http;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.SignalRHub
{
    [Authorize]
    public class SidebarHub : Hub
    {
        private readonly ISupportConversationService _supportConversationService;

        public SidebarHub(ISupportConversationService supportConversationService)
        {
            _supportConversationService = supportConversationService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            if (!Guid.TryParse(userId, out var staffId))
            {
                Context.Abort();
                return;
            }
            var httpContext = Context.GetHttpContext();
            var normalizedProvider = httpContext?.Request.Query["providerName"].ToString() ?? "";

            Console.WriteLine($"[SidebarHub] providerName: {normalizedProvider}");

            try
            {
                var conversations = await _supportConversationService
                    .GetStaffConversationSideBarAsync(staffId, normalizedProvider);

                await Clients.Caller.SendAsync("SidebarUpdated", conversations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SidebarHub] Error: {ex.Message}");
            }

            await base.OnConnectedAsync();
        }
    }
}
