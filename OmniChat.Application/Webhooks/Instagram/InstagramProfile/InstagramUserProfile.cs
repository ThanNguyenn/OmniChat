using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Webhooks.Instagram.InstagramProfile
{
    public class InstagramUserProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AccountType { get; set; }

        // optional – không luôn có
        public string? ProfilePictureUrl { get; set; }
        public bool? IsVerified { get; set; }
    }
}
