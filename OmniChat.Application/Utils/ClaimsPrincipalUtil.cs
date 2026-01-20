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
}
