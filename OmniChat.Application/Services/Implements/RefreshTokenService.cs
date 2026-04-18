using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class RefreshTokenService : BaseService<RefreshTokenService>, IRefreshTokenService
{
    public RefreshTokenService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<RefreshTokenService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration config) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task<string> CreateRefreshTokenAsync(Guid accountId, string sessionId)
    {
        string rawToken = GenerateRefreshToken();
        string hashedToken = HashRefreshToken(rawToken);

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var repo = _unitOfWork.GetRepository<RefreshToken>();
            var existingTokens = await repo.GetListAsync(predicate: t => t.AccountId == accountId);

            var refreshToken = new RefreshToken
            {
                AccountId = accountId,
                Token = hashedToken,
                UniqueIdentity = sessionId,
                ExpireDate = DateTime.UtcNow.AddDays(7),
                CreateDate = DateTime.UtcNow
            };

            await repo.InsertAsync(refreshToken);
        });

        return rawToken;
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private string HashRefreshToken(string refreshToken)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public async Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken)
    {
        var repo = _unitOfWork.GetRepository<RefreshToken>(); var sendedTokenHash = HashRefreshToken(refreshToken);

        var token = await repo.SingleOrDefaultAsync(predicate: t => t.Token == sendedTokenHash && t.ExpireDate > DateTime.UtcNow, include: q => q.Include(token => token.Account));
        return token;
    }

    public async Task<bool> InvalidateRefreshTokenAsync(string refreshToken)
    {
        var repo = _unitOfWork.GetRepository<RefreshToken>();
        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var sendedTokenHash = HashRefreshToken(refreshToken);
            var token = await repo.SingleOrDefaultAsync(predicate: t => t.Token == sendedTokenHash);
            if (token != null)
            {
                repo.Delete(token);
            }
        });
        return true;
    }


    public async Task DeleteExpiredRefreshTokensAsync()
    {
        var repo = _unitOfWork.GetRepository<RefreshToken>();
        await repo.DeleteAsync(predicate: t => t.ExpireDate <= DateTime.UtcNow);
    }

}



