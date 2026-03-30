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
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class ZaloUserService : BaseService<ZaloUserService>, IZaloUserService
    {
      
        private readonly HttpClient _httpClient;
        private readonly IZaloOAuthService _zaloOAuthService;

        public ZaloUserService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<ZaloUserService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IZaloOAuthService zaloOAuthService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _httpClient = httpClient;
            _zaloOAuthService = zaloOAuthService;
        }

        public async Task<ZaloUserProfileData?> GetUserProfileAsync(long zaloUserId)
        {
            try
            {
                var accessToken = await _zaloOAuthService.GetAccessTokenAsync();

                
                var dataObj = new
                {
                    user_id = zaloUserId.ToString()
                };

                var dataJson = JsonSerializer.Serialize(dataObj);
                var encodedData = Uri.EscapeDataString(dataJson);

                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"https://openapi.zalo.me/v3.0/oa/user/detail?data={encodedData}"
                );


                request.Headers.Add("access_token", accessToken);

                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<ZaloUserProfileResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result?.Error != 0)
                {
                    _logger.LogWarning("Zalo V3 getprofile failed: {Message}", result?.Message);
                    return null;
                }

                return result?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Zalo V3 user/detail API");
                throw;
            }
        }

        public  DateTime? ParseZaloBirthDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParseExact(
                value,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
            {
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }

            return null;
        }
    }
}
