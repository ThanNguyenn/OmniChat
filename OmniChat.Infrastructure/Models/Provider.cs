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

        public DateTime? CreateDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<SupportConversation> SupportConversations { get; set; } = new List<SupportConversation>();

        public virtual ICollection<FacebookOathToken> FacebookOathTokens { get; set; } = new List<FacebookOathToken>();

        public virtual ICollection<InstagramOathToken> InstagramOathTokens { get; set; } = new List<InstagramOathToken>();

        public virtual ICollection<ZaloOathToken> ZaloOathTokens { get; set; } = new List<ZaloOathToken>();
    }
}
