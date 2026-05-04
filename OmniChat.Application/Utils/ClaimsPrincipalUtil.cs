using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Utils;

public static class ClaimsPrincipalUtil
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        if (user == null)
            throw new UnauthorizedAccessException("User context is missing.");

        var userIdClaim = user.Claims.FirstOrDefault(c =>
            string.Equals(c.Type, "UserId", StringComparison.OrdinalIgnoreCase));


        if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            throw new UnauthorizedAccessException("Invalid UserId claim.");

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid UserId claim.");

        return userId;
    }

    public static Guid GetSubId(this ClaimsPrincipal user)
    {
        if (user == null)
            throw new UnauthorizedAccessException("User context is missing.");

        var subIdClaim = user.Claims.FirstOrDefault(c =>
            string.Equals(c.Type, "sub", StringComparison.OrdinalIgnoreCase));


        if (subIdClaim == null || string.IsNullOrWhiteSpace(subIdClaim.Value))
            throw new UnauthorizedAccessException("Invalid UserId claim.");

        if (!Guid.TryParse(subIdClaim.Value, out var subId))
            throw new UnauthorizedAccessException("Invalid UserId claim.");

        return subId;
    }

    public static string GetSessionId(this ClaimsPrincipal user)
    {
        if (user == null)
            throw new UnauthorizedAccessException("User context is missing.");

        var sessionClaim = user.Claims.FirstOrDefault(c =>
            string.Equals(c.Type, "session_id", StringComparison.OrdinalIgnoreCase));

        if (sessionClaim == null || string.IsNullOrWhiteSpace(sessionClaim.Value))
            throw new UnauthorizedAccessException("Invalid session_id claim.");

        return sessionClaim.Value;
    }

    public static string GetRole(this ClaimsPrincipal user)
    {
        if (user == null)
            throw new UnauthorizedAccessException("User context is missing.");

        var roleClaim = user.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Role ||
            string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase));

        if (roleClaim == null || string.IsNullOrWhiteSpace(roleClaim.Value))
            throw new UnauthorizedAccessException("Invalid role claim.");

        return roleClaim.Value;
    }
}
