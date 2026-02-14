using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.InstagramOauthToken;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class InstagramOAuthService : BaseService<InstagramOAuthService>, IInstagramOAuthService
    {
        public InstagramOAuthService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<InstagramOAuthService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }


        public async Task<bool> CreateInstagramOauthTokenAsync(InstagramOauthTokenRequest request)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                // call Repo 
                var repo = _unitOfWork.GetRepository<InstagramOathToken>();

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                // create entity
                var entity = _mapper.Map<InstagramOathToken>(request);

                // insert Database
                await repo.InsertAsync(entity);

                return true;

            });
        }

        private async Task<InstagramOathToken> GetInstagramOathTokenByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<InstagramOathToken>();

            var existInstagramOauthToken = await repo.SingleOrDefaultAsync(predicate: iot => iot.Id == id);

            if (existInstagramOauthToken == null)
                throw new NotFoundException(" Instagram token not found");

            return existInstagramOauthToken;
        }


        public async Task<bool> UpdateInstagramOathTokenAsync(Guid instagramTokenId, string newAccessToken)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var repo = _unitOfWork.GetRepository<InstagramOathToken>();

                var existToken = await GetInstagramOathTokenByIdAsync(instagramTokenId);

                existToken.AccessToken = newAccessToken;

                existToken.LastUpdateAt = DateTime.UtcNow;

                repo.Update(existToken);

                return true;
            });
        }

        public async Task<bool> DeleteInstagramTokenAsync(Guid instagramTokenId)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var repo = _unitOfWork.GetRepository<InstagramOathToken>();

                var existToken = await GetInstagramOathTokenByIdAsync(instagramTokenId);

                existToken.IsActive = false;

                existToken.AccessTokenExpiredDate = DateTime.UtcNow;

                existToken.LastUpdateAt = DateTime.UtcNow;

                repo.Update(existToken);

                return true;

            });
        }
    }
}
