using OmniChat.Infrastructure.Dtos.Requests.InstagramOauthToken;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IInstagramOAuthService
    {
        public  Task<bool> CreateInstagramOauthTokenAsync(InstagramOauthTokenRequest request);

        public  Task<bool> UpdateInstagramOathTokenAsync(Guid instagramTokenId, string newAccessToken);

        public  Task<bool> DeleteInstagramTokenAsync(Guid instagramTokenId);
    }
}
