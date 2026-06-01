using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class ZaloOAuthService : BaseService<ZaloOAuthService>, IZaloOAuthService
{
    private readonly IConfiguration _config;

    public ZaloOAuthService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<ZaloOAuthService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration config) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _config = config;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var token = await GetActiveTokenAsync();

        if (!NeedsRefresh(token))
            return token.AccessToken;
        return null;
    }

    public async Task RefreshAccessTokenAsync()
    {
        var appId = _config["ZaloWebHook:AppId"];
        var secretKey = _config["ZaloWebHook:AppSecretKey"];

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var accessToken = await GetActiveTokenAsync();

            if (!NeedsRefresh(accessToken))
                return;

            var (newAccessToken, newRefreshToken, expiresInSeconds) =
                await RefreshTokenFromZaloAsync(accessToken.RefreshToken, appId, secretKey);

            UpdateToken(accessToken, newAccessToken, newRefreshToken, expiresInSeconds);

            _unitOfWork.GetRepository<ZaloOathToken>().Update(accessToken);
        });
    }

    private async Task<ZaloOathToken> GetActiveTokenAsync()
    {
        var repo = _unitOfWork.GetRepository<ZaloOathToken>();
        var token = await repo.SingleOrDefaultAsync(predicate: t => t.IsActive == true) ?? throw new BusinessException("There is no active token");
        return token;
    }

    private bool NeedsRefresh(ZaloOathToken token) =>
        token.AccessTokenExpiredDate <= DateTime.UtcNow.AddHours(2);

    private async Task<(string accessToken, string refreshToken, int expiresIn)> RefreshTokenFromZaloAsync(
        string refreshToken, string appId, string secretKey)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("secret_key", secretKey);

        var content = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("refresh_token", refreshToken),
        new KeyValuePair<string, string>("app_id", appId),
        new KeyValuePair<string, string>("grant_type", "refresh_token")
    });

        var response = await http.PostAsync("https://oauth.zaloapp.com/v4/oa/access_token", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        //_logger.LogInformation("Zalo refresh response: {json}", json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        string newAccessToken = root.GetProperty("access_token").GetString()
            ?? throw new BusinessException("Missing access_token in Zalo response");
        string newRefreshToken = root.GetProperty("refresh_token").GetString()
            ?? throw new BusinessException("Missing refresh_token in Zalo response");

        int expiresIn = root.TryGetProperty("expires_in", out var expProp)
            ? expProp.ValueKind switch
            {
                JsonValueKind.String => int.TryParse(expProp.GetString(), out var val) ? val
                                   : throw new BusinessException("Invalid expires_in value in Zalo response"),
                JsonValueKind.Number => expProp.GetInt32(),
                _ => throw new BusinessException("Invalid expires_in type in Zalo response")
            }
            : throw new BusinessException("Missing expires_in in Zalo response");

        return (newAccessToken, newRefreshToken, expiresIn);
    }

    private void UpdateToken(ZaloOathToken token, string newAccess, string newRefresh, int expiresInSeconds)
    {
        token.AccessToken = newAccess;
        token.RefreshToken = newRefresh;
        token.AccessTokenExpiredDate = DateTime.UtcNow.AddSeconds(expiresInSeconds);
        token.RefreshTokenExpiredDate = DateTime.UtcNow.AddMonths(3);
        token.LastRefreshTokenAt = DateTime.UtcNow;
    }
}
