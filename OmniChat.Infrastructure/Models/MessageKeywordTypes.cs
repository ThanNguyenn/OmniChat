using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class MessageKeywordTypes
    {
        public Guid Id { get; set; }

        public Guid MessageId { get; set; }

        public virtual CustomerMessage CustomerMessage { get; set; }

        public Guid KeywordTypeId { get; set; }

        public virtual KeywordTypes KeywordTypes { get; set; }

        public virtual ICollection<MessageKeyword> MessageKeywords { get; set; } = new List<MessageKeyword>();
    }
}
