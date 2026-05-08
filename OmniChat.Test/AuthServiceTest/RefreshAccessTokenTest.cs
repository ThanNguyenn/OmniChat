using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Utils;
using OmniChat.Infrastructure.Dtos.Requests.Auth;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;  
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.AuthServiceTest;

public class RefreshAccessTokenTest
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    private readonly Mock<IHttpContextAccessor> _httpMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly Mock<IRefreshTokenService> _refreshServiceMock = new();

    private readonly IConfiguration _config;
    private readonly JwtUtil _jwtUtil;

    public RefreshAccessTokenTest()
    {
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-123456789123456789",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:TokenValidityInMinutes"] = "60"
            })
            .Build();

        _jwtUtil = new JwtUtil(_config);
    }

    private AuthService CreateService()
    {
        return new AuthService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _config,
            _refreshServiceMock.Object,
            _jwtUtil
        );
    }

    [Fact]
    public async Task RefreshAccessToken_ShouldReturnNewAccessToken_WhenRefreshTokenValid()
    {
        var accountId = Guid.NewGuid();

        var account = new Account
        {
            Id = accountId,
            Role = new Role
            {
                Name = "Staff"
            },
            Staff = new Staff
            {
                Id = Guid.NewGuid(),
                Name = "Test User"
            }
        };

        var refreshToken = new RefreshToken
        {
            AccountId = accountId,
            UniqueIdentity = Guid.NewGuid().ToString(),
            Account = account,
            ExpireDate = DateTime.UtcNow.AddDays(7)
        };

        _refreshServiceMock.Setup(r =>
            r.ValidateRefreshTokenAsync("valid-refresh-token"))
            .ReturnsAsync(refreshToken);

        var service = CreateService();

        var result = await service.RefreshAccessToken(
            new RefreshAccessTokenRequest
            {
                RefreshToken = "valid-refresh-token"
            });

        Assert.NotNull(result);

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));

        _refreshServiceMock.Verify(r =>
            r.ValidateRefreshTokenAsync("valid-refresh-token"),
            Times.Once);
    }

    [Fact]
    public async Task RefreshAccessToken_ShouldThrowUnauthorized_WhenRefreshTokenInvalid()
    {
        _refreshServiceMock.Setup(r =>
            r.ValidateRefreshTokenAsync("invalid-refresh-token"))
            .ReturnsAsync((RefreshToken?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.RefreshAccessToken(
                new RefreshAccessTokenRequest
                {
                    RefreshToken = "invalid-refresh-token"
                }));

        _refreshServiceMock.Verify(r =>
            r.ValidateRefreshTokenAsync("invalid-refresh-token"),
            Times.Once);
    }

    [Fact]
    public async Task RefreshAccessToken_ShouldThrowUnauthorized_WhenRefreshTokenExpired()
    {
        _refreshServiceMock.Setup(r =>
            r.ValidateRefreshTokenAsync("expired-refresh-token"))
            .ReturnsAsync((RefreshToken?)null); // expired => treated as invalid

        var service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.RefreshAccessToken(
                new RefreshAccessTokenRequest
                {
                    RefreshToken = "expired-refresh-token"
                }));

        _refreshServiceMock.Verify(r =>
            r.ValidateRefreshTokenAsync("expired-refresh-token"),
            Times.Once);
    }
}
