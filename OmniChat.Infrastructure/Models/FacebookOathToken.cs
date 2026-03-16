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

        public DateTime? AccessTokenExpiredDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdateAt { get; set; } = DateTime.UtcNow;

        public bool? IsActive { get; set; }

        public Guid? ProviderId { get; set; }

        public virtual Provider? Provider { get; set; }
    }
}
