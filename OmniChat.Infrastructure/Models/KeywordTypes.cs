using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class KeywordTypes
    {
        public Guid Id { get; set; }
        
        public string TypeName { get; set; }

        public string Description { get; set; }

        public bool? IsActive { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

        public virtual ICollection<Keyword> Keywords { get; set; } = new List<Keyword>();

        public virtual ICollection<MessageKeywordTypes> MessageKeywordTypes { get; set; } = new List<MessageKeywordTypes>();

        public virtual ICollection<SupportTask> SupportTasks { get; set; } = new List<SupportTask>();

        public virtual ICollection<Staff> Staffs { get; set; } = new List<Staff>();
    }
}
