using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Claim = System.Security.Claims.Claim;

namespace OmniChat.Test.AuthServiceTest;

public class ChangePasswordAsyncTest
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    private readonly Mock<IHttpContextAccessor> _httpMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly JwtUtil _jwtUtil;
    private readonly Mock<IRefreshTokenService> _refreshService = new();

    public ChangePasswordAsyncTest()
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

    private void SetupHttpContext(Guid userId)
    {
        var claims = new List<Claim>
    {
        new Claim("UserId", userId.ToString())
    };

        var identity = new ClaimsIdentity(claims, "mock");

        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = principal
        };

        _httpMock.Setup(x => x.HttpContext)
            .Returns(context);
    }

    private Mock<IGenericRepository<Account>> SetupAccountRepo(Account? account)
    {
        var repoMock = new Mock<IGenericRepository<Account>>();

        repoMock.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Account, bool>>>(),
            It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>?>(),
            It.IsAny<Func<IQueryable<Account>, IIncludableQueryable<Account, object>>?>()
        )).ReturnsAsync(account);

        repoMock.Setup(r => r.Update(It.IsAny<Account>()));

        _uowMock.Setup(u => u.GetRepository<Account>())
            .Returns(repoMock.Object);

        _uowMock.Setup(u => u.ProcessInTransactionAsync(
            It.IsAny<Func<Task>>()
        )).Returns<Func<Task>>(async action => await action());

        return repoMock;
    }

    private async Task<Account> CreateValidAccount(Guid userId, string password)
    {
        return new Account
        {
            Id = userId,
            UserName = "test@gmail.com",
            Password = await PasswordUtil.HashPassword(password)
        };
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldUpdatePassword_WhenOldPasswordCorrect()
    {
        var userId = Guid.NewGuid();

        SetupHttpContext(userId);

        var account = await CreateValidAccount(userId, "old-password");

        var repoMock = SetupAccountRepo(account);

        var service = CreateService();

        var result = await service.ChangePasswordAsync(
            new ChangePasswordResquest
            {
                OldPassword = "old-password",
                NewPassword = "new-password"
            });

        Assert.True(result);

        Assert.True(
            await PasswordUtil.VerifyPassword(
                "new-password",
                account.Password));

        repoMock.Verify(r => r.Update(
            It.Is<Account>(a => a.Id == userId)
        ), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldThrow_WhenOldPasswordIncorrect()
    {
        var userId = Guid.NewGuid();

        SetupHttpContext(userId);

        var account = await CreateValidAccount(userId, "correct-password");

        var repoMock = SetupAccountRepo(account);

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.ChangePasswordAsync(
                new ChangePasswordResquest
                {
                    OldPassword = "wrong-password",
                    NewPassword = "new-password"
                }));

        repoMock.Verify(r => r.Update(
            It.IsAny<Account>()
        ), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldThrow_WhenAccountNotFound()
    {
        var userId = Guid.NewGuid();

        SetupHttpContext(userId);

        var repoMock = SetupAccountRepo(null);

        var service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.ChangePasswordAsync(
                new ChangePasswordResquest
                {
                    OldPassword = "old-password",
                    NewPassword = "new-password"
                }));

        repoMock.Verify(r => r.Update(
            It.IsAny<Account>()
        ), Times.Never);
    }
}
