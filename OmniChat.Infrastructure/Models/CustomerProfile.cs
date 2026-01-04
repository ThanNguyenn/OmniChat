using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class CustomerProfile
    {
        public Guid Id { get; set; }

        public string CustomerName { get; set; }

        public Guid ProvidersId { get; set; }

        public virtual Provider Providers { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public bool Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? AvatarUrl { get; set; }

        public long SenderId { get; set; }

        public virtual ICollection<CustomerMessage> CustomerMessages { get; set; } = new List<CustomerMessage>();

        public virtual ICollection<SupportConversation> SupportConversations { get; set; } = new List<SupportConversation>();
    }
}
