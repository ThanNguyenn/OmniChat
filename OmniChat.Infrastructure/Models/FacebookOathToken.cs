using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class FacebookOathToken
    {
        public Guid Id { get; set; }

        public string AccessToken { get; set; }

        public string AccessTokenExpiredDate { get; set; }

        public DateTime LastUpdateAt { get; set; }

        public bool? IsActive { get; set; }
    }
}
