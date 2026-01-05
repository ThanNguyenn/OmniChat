using OmniChat.Application.Webhooks.Facebook.FacebookProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IFacebookUserService
    {
     public Task<FacebookUserProfile?> GetUserProfileAsync(long psid);

    }
}
