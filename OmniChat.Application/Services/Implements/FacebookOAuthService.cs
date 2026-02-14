using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.FacebookOauthToken;
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
    public class FacebookOAuthService : BaseService<FacebookOAuthService>, IFacebookOAuthService
    {
       
        public FacebookOAuthService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<FacebookOAuthService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

         public async Task<bool> CreateNewFacebookTokenAsync(FacebookOauthTokenRequest request)
         {
                return await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    if (request == null)
                        throw new ArgumentNullException(nameof(request));

                    var repo = _unitOfWork.GetRepository<FacebookOathToken>();

                    var entity =  _mapper.Map<FacebookOathToken>(request);

                    await repo.InsertAsync(entity);

                    return true;
                });
         }

        private async Task<FacebookOathToken> GetFacebookOathTokenByIdAsync(Guid id)
        {
            var repo =  _unitOfWork.GetRepository<FacebookOathToken>();
            var existToken = await  repo.SingleOrDefaultAsync(predicate: fot => fot.Id == id);

            if (existToken == null)
                throw new NotFoundException("Facebook token not found");

            return existToken;
        }


        public async Task<bool> UpdateFacebookTokenAsync(Guid FacebookOathTokenId, string newAccessToken)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var repo = _unitOfWork.GetRepository<FacebookOathToken>();

                var existoken = await GetFacebookOathTokenByIdAsync(FacebookOathTokenId);

                existoken.AccessToken = newAccessToken;

                existoken.LastUpdateAt = DateTime.UtcNow;

                 repo.Update(existoken);

                return true;
            });
        }

        public async Task<bool> DeleteFacebookTokenAsync(Guid FacebookOathTokenId)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var repo = _unitOfWork.GetRepository<FacebookOathToken>();

                var existoken = await GetFacebookOathTokenByIdAsync(FacebookOathTokenId);

                existoken.IsActive = false;

                existoken.AccessTokenExpiredDate = DateTime.UtcNow;

                existoken.LastUpdateAt = DateTime.UtcNow;

                repo.Update(existoken);

                return true;
            });
        }
    }
}
