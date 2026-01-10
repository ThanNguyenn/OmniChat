using OmniChat.Application.Webhooks.Instagram.InstagramProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IInstagramUserService
    {
        public  Task<InstagramUserProfile?> GetUserProfileAsync(string instagramUserId);
    }
}
