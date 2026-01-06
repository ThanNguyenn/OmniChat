using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IRefreshTokenService
{
    public Task<string> CreateRefreshTokenAsync(Guid accountId);

    public Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken);

    public Task InvalidateRefreshTokenAsync(string refreshToken);
}
