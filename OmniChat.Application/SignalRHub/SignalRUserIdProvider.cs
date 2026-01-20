using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.SignalRHub
{
    public class SignalRUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // phải trùng với ActiveStaffId
            var userId =  connection.User?.FindFirst("sub")?.Value
            ?? connection.User?.FindFirst("UserId")?.Value
            ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Console.WriteLine($"[SignalRUserIdProvider] Resolved UserId: {userId}");
            return userId;
        }
    }
}
