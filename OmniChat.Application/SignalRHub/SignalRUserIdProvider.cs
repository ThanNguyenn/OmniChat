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
            Console.WriteLine("[SignalRUserIdProvider] === START ===");

            // Debug tất cả claims
            var allClaims = connection.User?.Claims.Select(c => $"{c.Type} = {c.Value}").ToList();
            if (allClaims != null && allClaims.Any())
            {
                Console.WriteLine("[SignalRUserIdProvider] All claims:");
                foreach (var claim in allClaims)
                {
                    Console.WriteLine($"  {claim}");
                }
            }
            else
            {
                Console.WriteLine("[SignalRUserIdProvider] NO CLAIMS FOUND!");
            }

            // Thử lấy các claim
            var subClaim = connection.User?.FindFirst("sub")?.Value;
            var userIdClaim = connection.User?.FindFirst("UserId")?.Value;
            var nameIdClaim = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Console.WriteLine($"[SignalRUserIdProvider] sub: {subClaim ?? "NULL"}");
            Console.WriteLine($"[SignalRUserIdProvider] UserId: {userIdClaim ?? "NULL"}");
            Console.WriteLine($"[SignalRUserIdProvider] NameIdentifier: {nameIdClaim ?? "NULL"}");

            var userId = subClaim ?? userIdClaim ?? nameIdClaim;
            Console.WriteLine($"[SignalRUserIdProvider] Final UserId: {userId ?? "NULL"}");
            Console.WriteLine("[SignalRUserIdProvider] === END ===");

            return userId;
        }
    }
}
