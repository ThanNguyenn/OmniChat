using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class ConversationFile
    {
        public Guid Id { get; set; }

        public string Url { get; set; }

        public string FileName { get; set; }

        public DateTime? TimeStamp { get; set; } = DateTime.UtcNow;

        public string Type { get; set; }

        public virtual ICollection<SupportConversationFile> SupportConversationFiles { get; set; } = new List<SupportConversationFile>();
    }
}
