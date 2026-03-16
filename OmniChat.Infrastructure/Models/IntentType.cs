using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class IntentType
    {
        public Guid Id { get; set; }
        
        public string TypeName { get; set; }

        public string Description { get; set; }

        public bool? IsActive { get; set; }

        public int IntentTypePiority { get; set; }

        public DateTime? CreateDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Keyword> Keywords { get; set; } = new List<Keyword>();

        public virtual ICollection<MessageIntentType> MessageIntentTypes { get; set; } = new List<MessageIntentType>();

        public virtual ICollection<SupportTask> SupportTasks { get; set; } = new List<SupportTask>();

        public virtual ICollection<StaffIntentType> StaffIntentTypes { get; set; } = new List<StaffIntentType>();
    }
}
