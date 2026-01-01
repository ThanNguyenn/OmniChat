using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Provider
    {
        public Guid Id { get; set; }

        public string ProviderName { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual ICollection<CustomerProfile> CustomerProfiles { get; set; } = new List<CustomerProfile>();

        public virtual ICollection<SupportConversation> SupportConversations { get; set; } = new List<SupportConversation>();
    }
}
