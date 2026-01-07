using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Webhooks.Facebook.FacebookProfile;
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
    public class FacebookUserService : BaseService<FacebookUserService>,IFacebookUserService
    {
        private readonly HttpClient _httpClient;

        private readonly IConfiguration _configuration;

        public FacebookUserService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<FacebookUserService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor,HttpClient httpClient, IConfiguration configuration) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<FacebookUserProfile?> GetUserProfileAsync(long psid)
        {
            var pageAccessToken = _configuration["facebookWebHook:AccessToken"];

            // parse long -> string and prepend "act_"
            var psidString = $"{psid}";

            var url =
                $"https://graph.facebook.com/v18.0/{psidString}" +
                $"?fields=first_name,last_name,profile_pic,gender" +
                $"&access_token={pageAccessToken}";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "Facebook raw response. StatusCode: {StatusCode}, Body: {Body}",
                response.StatusCode,
                json
            );

            if (!response.IsSuccessStatusCode)
                return null;

            return JsonSerializer.Deserialize<FacebookUserProfile>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
    }
}
