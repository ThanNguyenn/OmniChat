using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class MessageKeyword
    {
        public Guid Id { get; set; }

        public DateTime StartedDate { get; set; }

        public DateTime EndedDate { get; set; }

        public Guid KeywordId { get; set; }

        public virtual Keyword Keyword { get; set; }

        public Guid MessageKeywordTypesId { get; set; }

        public virtual MessageKeywordTypes MessageKeywordTypes { get; set; }
    }
}
