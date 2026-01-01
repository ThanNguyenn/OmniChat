using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public string Token { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ExpireDate { get; set; }

        public Guid AccountId { get; set; }

        public virtual Account Account { get; set; }

    }
}
