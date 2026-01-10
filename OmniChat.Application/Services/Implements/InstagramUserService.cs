using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Webhooks.Facebook.FacebookProfile;
using OmniChat.Application.Webhooks.Instagram.InstagramProfile;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class InstagramUserService : BaseService<InstagramUserService>, IInstagramUserService
    {
        private readonly HttpClient _httpClient;

        private readonly IConfiguration _configuration;
        public InstagramUserService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<InstagramUserService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IConfiguration configuration) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<InstagramUserProfile?> GetUserProfileAsync(string instagramUserId)
        {
            var accessToken = _configuration["InstagramWebhook:AccessToken"];
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogError("Instagram access token is missing");
                return null;
            }
            var url =
                $"https://graph.facebook.com/v19.0/{instagramUserId}" +
                $"?fields=id,username,account_type,profile_picture_url"+
                 $"&access_token={accessToken}";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "Instagram raw response. StatusCode: {StatusCode}, Body: {Body}",
                response.StatusCode,
                json
            );

            if (!response.IsSuccessStatusCode)
                return null;

            return JsonSerializer.Deserialize<InstagramUserProfile>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

        }

    }
}
