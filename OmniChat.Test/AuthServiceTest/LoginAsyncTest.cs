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
using System.Linq.Expressions;

namespace OmniChat.Test.AuthServiceTest;

public class LoginAsyncTest
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    private readonly Mock<IHttpContextAccessor> _httpMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();

    private AuthService CreateService(
        JwtUtil jwtUtil,
        IConfiguration config,
        IRefreshTokenService refreshService)
    {
        return new AuthService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            config,
            refreshService,
            jwtUtil
        );
    }

    private async Task<Account> CreateValidAccount(Guid accountId)
    {
        return new Account
        {
            Id = accountId,
            UserName = "khoanam@gmail.com",
            Password = await PasswordUtil.HashPassword("Omnichat@0294"),
            Role = new Role { Name = "Staff" },
            Staff = new Staff { Id = Guid.NewGuid(), Name = "Khoa Nam" }
        };
    }

    private (
        Mock<IGenericRepository<Account>>,
        Mock<IGenericRepository<RefreshToken>>
    )
        SetupUnitOfWork(Account? account = null)
    {
        var accountRepoMock = new Mock<IGenericRepository<Account>>();
        accountRepoMock.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Account, bool>>>(),
            It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>?>(),
            It.IsAny<Func<IQueryable<Account>, IIncludableQueryable<Account, object>>?>()
        )).ReturnsAsync(account);

        var refreshRepoMock = new Mock<IGenericRepository<RefreshToken>>();
        refreshRepoMock.Setup(r => r.GetListAsync(
            It.IsAny<Expression<Func<RefreshToken, bool>>>(),
            It.IsAny<Func<IQueryable<RefreshToken>, IOrderedQueryable<RefreshToken>>?>(),
            It.IsAny<Func<IQueryable<RefreshToken>, IIncludableQueryable<RefreshToken, object>>?>()
        )).ReturnsAsync(new List<RefreshToken>());

        refreshRepoMock.Setup(r => r.InsertAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _uowMock.Setup(u => u.GetRepository<Account>())
            .Returns(accountRepoMock.Object);

        _uowMock.Setup(u => u.GetRepository<RefreshToken>())
            .Returns(refreshRepoMock.Object);

        _uowMock.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());

        return (accountRepoMock, refreshRepoMock);
    }

    private AuthService BuildService(
    Account account,
    out Mock<IGenericRepository<Account>> accountRepoMock,
    out Mock<IGenericRepository<RefreshToken>> refreshRepoMock)
    {
        (accountRepoMock, refreshRepoMock) = SetupUnitOfWork(account);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-123456789123456789",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:TokenValidityInMinutes"] = "60"
            })
            .Build();

        var jwtUtil = new JwtUtil(config);

        var refreshService = new RefreshTokenService(
            _uowMock.Object,
            new Mock<ILogger<RefreshTokenService>>().Object,
            _mapperMock.Object,
            _httpMock.Object,
            config
        );

        return CreateService(jwtUtil, config, refreshService);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenValid()
    {
        var accountId = Guid.NewGuid();
        var account = await CreateValidAccount(accountId);

        var service = BuildService(
            account,
            out var accountRepoMock,
            out var refreshRepoMock);

        var result = await service.LoginAsync(new LoginRequest
        {
            Username = account.UserName,
            Password = "Omnichat@0294"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        Assert.Equal("Staff", result.Role);

        accountRepoMock.Verify(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Account, bool>>>(),
            It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>?>(),
            It.IsAny<Func<IQueryable<Account>, IIncludableQueryable<Account, object>>?>()
        ), Times.Once);

        refreshRepoMock.Verify(r => r.InsertAsync(
            It.Is<RefreshToken>(t =>
                t.AccountId == accountId &&
                !string.IsNullOrEmpty(t.Token) &&
                !string.IsNullOrEmpty(t.UniqueIdentity)
            )
        ), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenUsernameIsWrong()
    {
        var service = BuildService(
            null,
            out _,
            out var refreshRepoMock);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequest
            {
                Username = "WrongUserName",
                Password = "Omnichat@0294"
            }));

        refreshRepoMock.Verify(
            r => r.InsertAsync(It.IsAny<RefreshToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenPasswordIsWrong()
    {
        var accountId = Guid.NewGuid();
        var account = await CreateValidAccount(accountId);

        var service = BuildService(
            account,
            out var accountRepoMock,
            out var refreshRepoMock);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequest
            {
                Username = account.UserName,
                Password = "Omnichat@0293"
            }));
        accountRepoMock.Verify(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Account, bool>>>(),
            It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>?>(),
            It.IsAny<Func<IQueryable<Account>, IIncludableQueryable<Account, object>>?>()
        ), Times.Once);
        refreshRepoMock.Verify(
            r => r.InsertAsync(It.IsAny<RefreshToken>()),
            Times.Never);
    }
}
