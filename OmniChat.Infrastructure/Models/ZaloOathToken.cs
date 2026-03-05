using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class ZaloOathToken
    {
        public Guid Id { get; set; }

        public string AccessToken { get; set; }

        public DateTime AccessTokenExpiredDate { get; set; }

        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiredDate { get; set; }

        public DateTime LastRefreshTokenAt { get; set; }

        public bool? IsActive { get; set; }

        public Guid? ProviderId { get; set; }

        public virtual Provider? Provider { get; set; }
    }
}
