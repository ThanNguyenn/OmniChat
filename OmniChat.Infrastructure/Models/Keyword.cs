using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Keyword
    {
        public Guid Id { get; set; }

        public string KeywordText { get; set; }

        public DateTime CreateDate { get; set; }

        public bool? IsDeleted { get; set; }

        public string Code { get; set; }
        
        public float Weight { get; set; }

        public Guid KeyWordTypeId { get; set; }

        public virtual KeywordTypes KeyWordType { get; set; }

        public virtual ICollection<MessageKeyword> MessageKeywords { get; set; } = new List<MessageKeyword>();
    }
}
