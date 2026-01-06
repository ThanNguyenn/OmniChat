using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IRefreshTokenService
{
    Task<string> CreateRefreshTokenAsync(Guid accountId);

    Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken);

    Task<bool> InvalidateRefreshTokenAsync(string refreshToken);

    Task DeleteExpiredRefreshTokensAsync();
}
