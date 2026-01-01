using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Role
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
