using OmniChat.Infrastructure.Dtos.Requests.Auth;
using OmniChat.Infrastructure.Dtos.Responses.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
    Task<bool> LogoutAsync();
    Task<bool> ChangePasswordAsync(ChangePasswordResquest changePasswordRequest);
    Task<RefreshAccessTokenResponse> RefreshAccessToken(RefreshAccessTokenRequest refreshAccessTokenRequest);
}
