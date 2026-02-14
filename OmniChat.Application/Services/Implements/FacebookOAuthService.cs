using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
    public class FacebookOAuthService : BaseService<FacebookOAuthService>
    {
        public FacebookOAuthService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<FacebookOAuthService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        //public async Task<string> GetAccessTokenAsync()
        //{
        //    var accessToken = await GetActiveTokenAsync();
           


        //}

        //private bool NeedToRefesh(FacebookOathToken token)
        //{
        //    if (token.AccessTokenExpiredDate <= DateTime.UtcNow.AddMinutes(5))
        //    {
        //        return true;
        //    }
        //    return false;
        //}
    }
}
