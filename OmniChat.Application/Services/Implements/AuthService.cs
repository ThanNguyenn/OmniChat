using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Utils;
using OmniChat.Infrastructure.Dtos.Requests.Auth;
using OmniChat.Infrastructure.Dtos.Responses.Auth;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class AuthService : BaseService<AuthService>, IAuthService
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JwtUtil _jwtUtil;

    public AuthService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<AuthService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration config, IRefreshTokenService refreshTokenService, JwtUtil jwtUtil) : base(unitOfWork, logger, mapper, httpContextAccessor) 
    { 
        _refreshTokenService = refreshTokenService;
        _jwtUtil = jwtUtil;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();

        var account = await accountRepo.SingleOrDefaultAsync(predicate: a => a.UserName == loginRequest.Username || a.Staff.Email == loginRequest.Username, include: q => q.Include(a => a.Role).Include(a => a.Staff)) ?? throw new UnauthorizedException("Username or password incorect");

        if (!await PasswordUtil.VerifyPassword(loginRequest.Password,account.Password))
        {
            throw new UnauthorizedException("Username or password incorect");
        }

        var guidSecurityClaim = new Tuple<string, Guid>("UserId", account.Id);
        var sessionId = Guid.NewGuid().ToString();
        var accessToken = _jwtUtil.GenerateJwtToken(account, guidSecurityClaim, sessionId);

        var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(account.Id, sessionId);

        var loginResponse = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Role = account.Role.Name,
            AccountId =  account.Id,
            StaffId = account.Staff.Id
        };
        return loginResponse;
    }   

    public async Task<bool> LogoutAsync()
    {
        var sessionId = _httpContextAccessor.HttpContext!.User.GetSessionId();
        var repo = _unitOfWork.GetRepository<RefreshToken>();
        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var tokens = await repo.GetListAsync(predicate: t => t.UniqueIdentity == sessionId);
            if (tokens.Any())
            {
                repo.DeleteRange(tokens);
            }
        });
        return true;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordResquest request)
    {
        var accountId = _httpContextAccessor.HttpContext!.User.GetUserId();
        var repo = _unitOfWork.GetRepository<Account>();

        var account = await repo.SingleOrDefaultAsync(predicate: a => a.Id == accountId) ??
            throw new UnauthorizedException("Account not found.");

        if (!await PasswordUtil.VerifyPassword(request.OldPassword, account.Password))
        {
            throw new UnauthorizedException("Old password is incorrect.");
        }

        var newHashedPassword = await PasswordUtil.HashPassword(request.NewPassword);

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            account.Password = newHashedPassword;
            repo.Update(account);
        });

        return true;
    }

    public async Task<RefreshAccessTokenResponse> RefreshAccessToken(RefreshAccessTokenRequest refreshAccessTokenRequest)
    {
        var token = await _refreshTokenService.ValidateRefreshTokenAsync(refreshAccessTokenRequest.RefreshToken);
        if (token == null)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }
        var guidSecurityClaim = new Tuple<string, Guid>("UserId", token.AccountId);
        var newAccessToken = _jwtUtil.GenerateJwtToken(token.Account, guidSecurityClaim, token.UniqueIdentity);

        var response = new RefreshAccessTokenResponse
        {
            AccessToken = newAccessToken
        };
        return response;
    }
}
