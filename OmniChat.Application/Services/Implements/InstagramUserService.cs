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
using System.Net.Http.Headers;
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
            var accessToken = _configuration["InstagramWebhook:InstagramPageAccessToken"];
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogError("Instagram access token is missing");
                return null;
            }

            var businessAccountId = _configuration["InstagramWebhook:BusinessId"];

            if (string.IsNullOrWhiteSpace(businessAccountId))
            {
                _logger.LogError("Instagram BusinessAccount Id is missing");
                return null;
            }
            var url =
                $"https://graph.facebook.com/v24.0/{instagramUserId}" +
                $"?fields=name,profile_pic" +
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

        //public async Task<InstagramUserProfile?> GetUserProfileAsync(string instagramUserId)
        //{
        //    var accessToken = _configuration["InstagramWebhook:InstagramPageAccessToken"]?.Trim();
        //    if (string.IsNullOrEmpty(accessToken))
        //    {
        //        _logger.LogError("Instagram access token is missing");
        //        return null;
        //    }

        //    var request = new HttpRequestMessage(
        //        HttpMethod.Get,
        //        $"https://graph.instagram.com/v24.0/{instagramUserId}?fields=id,username,account_type,profile_picture_url"
        //    );

        //    request.Headers.Authorization =
        //        new AuthenticationHeaderValue("Bearer", accessToken);

        //    var response = await _httpClient.SendAsync(request);
        //    var json = await response.Content.ReadAsStringAsync();

        //    _logger.LogInformation(
        //        "[INSTAGRAM][PROFILE] StatusCode={StatusCode}, Body={Body}",
        //        response.StatusCode,
        //        json
        //    );

        //    if (!response.IsSuccessStatusCode)
        //        return null;

        //    return JsonSerializer.Deserialize<InstagramUserProfile>(
        //        json,
        //        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        //    );
        //}


    }
}
