using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Account
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Passsword { get; set; }

        public string UserName { get; set; }

        public string Phone { get; set; }

        public bool IsActive { get; set; }

        public Guid RoleId { get; set; }

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public virtual Staff Staff { get; set; }

        public virtual Role Role { get; set; }

    }
}
