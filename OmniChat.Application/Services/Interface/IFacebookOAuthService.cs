using OmniChat.Infrastructure.Dtos.Requests.FacebookOauthToken;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IFacebookOAuthService
    {
        public  Task<bool> CreateNewFacebookTokenAsync(FacebookOauthTokenRequest request);

        public  Task<bool> UpdateFacebookTokenAsync(Guid FacebookOathTokenId, string newAccessToken);

        public  Task<bool> DeleteFacebookTokenAsync(Guid FacebookOathTokenId);


    }
}
