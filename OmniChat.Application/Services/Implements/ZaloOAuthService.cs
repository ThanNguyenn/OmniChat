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

    public async Task RefreshAccessTokenAsync()
    {
        var appId = _config["ZaloWebHook:AppId"];
        var secretKey = _config["ZaloWebHook:AppSecretKey"];

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var accessToken = await _unitOfWork.GetRepository<ZaloOathToken>()
                .SingleOrDefaultAsync(predicate: q => q.IsActive == true);

            if (accessToken == null)
            {
                accessToken = new ZaloOathToken
                {
                    Id = Guid.NewGuid(),
                    AccessToken = string.Empty,
                    RefreshToken = string.Empty,
                    AccessTokenExpiredDate = DateTime.UtcNow,
                    RefreshTokenExpiredDate = DateTime.UtcNow,
                    LastRefreshTokenAt = DateTime.UtcNow,
                    IsActive = true
                };
                _unitOfWork.GetRepository<ZaloOathToken>().InsertAsync(accessToken);
            }

            if (accessToken.AccessTokenExpiredDate > DateTime.UtcNow.AddMinutes(5))
            {
                return;
            }

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("secret_key", secretKey);

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("refresh_token", accessToken.RefreshToken),
            new KeyValuePair<string, string>("app_id", appId),
            new KeyValuePair<string, string>("grant_type", "refresh_token")
        });

            var response = await http.PostAsync("https://oauth.zaloapp.com/v4/oa/access_token", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Zalo refresh response: {json}", json);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var newAccessToken = root.TryGetProperty("access_token", out var atProp)
                ? atProp.GetString()
                : throw new BusinessException("Missing access_token in Zalo response");

            var newRefreshToken = root.TryGetProperty("refresh_token", out var rtProp)
                ? rtProp.GetString()
                : throw new BusinessException("Missing refresh_token in Zalo response");


            int expiresInSeconds = 0;
            if (root.TryGetProperty("expires_in", out var expProp))
            {
                if (expProp.ValueKind == JsonValueKind.String)
                {
                    if (!int.TryParse(expProp.GetString(), out expiresInSeconds))
                        throw new BusinessException("Invalid expires_in value in Zalo response");
                }
                else if (expProp.ValueKind == JsonValueKind.Number)
                {
                    expiresInSeconds = expProp.GetInt32();
                }
            }
            else
            {
                throw new BusinessException("Missing expires_in in Zalo response");
            }

            accessToken.AccessToken = newAccessToken!;
            accessToken.RefreshToken = newRefreshToken!;
            accessToken.AccessTokenExpiredDate = DateTime.UtcNow.AddSeconds(expiresInSeconds);
            accessToken.RefreshTokenExpiredDate = DateTime.UtcNow.AddMonths(3);
            accessToken.LastRefreshTokenAt = DateTime.UtcNow;

            _unitOfWork.GetRepository<ZaloOathToken>().Update(accessToken);
        });
    }
}
