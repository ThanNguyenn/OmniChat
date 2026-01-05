using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class ClaimType
    {
        public Guid Id { get; set; }

        public string TypeName { get; set; }

        public bool? IsActive { get; set; }

        public virtual ICollection<Claim>? Claims { get; set; }
    }
}
