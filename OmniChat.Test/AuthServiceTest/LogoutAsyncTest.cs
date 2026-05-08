using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Utils;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Claim = System.Security.Claims.Claim;

namespace OmniChat.Test.AuthServiceTest;

public class LogoutAsyncTest
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    private readonly Mock<IHttpContextAccessor> _httpMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly JwtUtil _jwtUtil;
    private readonly Mock<IRefreshTokenService> _refreshService = new();

    public LogoutAsyncTest()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-123456789123456789",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:TokenValidityInMinutes"] = "60"
            })
            .Build();

        _jwtUtil = new JwtUtil(config);

        _configurationMock.Setup(c => c["Jwt:Key"])
            .Returns("test-secret-key-123456789123456789");
    }

    private AuthService CreateService()
    {
        return new AuthService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _configurationMock.Object,
            _refreshService.Object,
            _jwtUtil
        );
    }

    private void SetupHttpContext(Guid sessionId)
    {
        var claims = new List<Claim>
        {
            new Claim("session_id", sessionId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "mock");
        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = principal
        };

        _httpMock.Setup(x => x.HttpContext).Returns(context);
    }

    private Mock<IGenericRepository<RefreshToken>> SetupRefreshRepo(List<RefreshToken> data)
    {
        var repoMock = new Mock<IGenericRepository<RefreshToken>>();

        repoMock.Setup(r => r.GetListAsync(
            It.IsAny<Expression<Func<RefreshToken, bool>>>(),
            It.IsAny<Func<IQueryable<RefreshToken>, IOrderedQueryable<RefreshToken>>?>(),
            It.IsAny<Func<IQueryable<RefreshToken>, IIncludableQueryable<RefreshToken, object>>?>()
        )).ReturnsAsync(data);

        repoMock.Setup(r => r.DeleteRange(It.IsAny<IEnumerable<RefreshToken>>()));

        _uowMock.Setup(u => u.GetRepository<RefreshToken>())
            .Returns(repoMock.Object);

        _uowMock.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());

        return repoMock;
    }

    [Fact]
    public async Task LogoutAsync_ShouldDeleteTokens_WhenExist()
    {
        var sessionId = Guid.NewGuid();

        SetupHttpContext(sessionId);

        var tokens = new List<RefreshToken>
    {
        new RefreshToken { UniqueIdentity = sessionId.ToString() },
        new RefreshToken { UniqueIdentity = sessionId.ToString() }
    };

        var repoMock = SetupRefreshRepo(tokens);

        var service = CreateService();

        await service.LogoutAsync();

        repoMock.Verify(r => r.DeleteRange(
            It.Is<IEnumerable<RefreshToken>>(t => t.Count() == 2)
        ), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldNotDelete_WhenNoTokensExist()
    {
        var sessionId = Guid.NewGuid();

        SetupHttpContext(sessionId);

        var repoMock = SetupRefreshRepo(new List<RefreshToken>());

        var service = CreateService();

        var result = await service.LogoutAsync();

        Assert.True(result);

        repoMock.Verify(r => r.GetListAsync(
            It.IsAny<Expression<Func<RefreshToken, bool>>>(),
            It.IsAny<Func<IQueryable<RefreshToken>, IOrderedQueryable<RefreshToken>>?>(),
            It.IsAny<Func<IQueryable<RefreshToken>, IIncludableQueryable<RefreshToken, object>>?>()
        ), Times.Once);

        repoMock.Verify(r => r.DeleteRange(
            It.IsAny<IEnumerable<RefreshToken>>()
        ), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_ShouldThrow_WhenSessionClaimMissing()
    {
        var identity = new ClaimsIdentity();

        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = principal
        };

        _httpMock.Setup(x => x.HttpContext)
            .Returns(context);

        var service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LogoutAsync());
    }
}
