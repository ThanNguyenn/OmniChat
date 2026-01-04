using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Webhooks.Zalo.ZaloProflie;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    internal class ZaloUserService : BaseService<ZaloUserService>, IZaloUserService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ZaloUserService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<ZaloUserService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, HttpClient httpClient) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<ZaloUserProfileData?> GetUserProfileAsync(long zaloUserId)
        {
            try
            {
                // fix call Database
                var accessToken = _configuration["ZaloOA:AccessToken"];

                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"https://openapi.zalo.me/v2.0/oa/getprofile?user_id={zaloUserId}"
                );

                request.Headers.Add("access_token", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<ZaloUserProfileResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result?.Error != 0)
                {
                    _logger.LogWarning(
                        "Zalo getprofile failed: {Message}",
                        result?.Message);
                    return null;
                }

                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error calling Zalo getprofile API");
                throw;
            }
        }
    }
}
