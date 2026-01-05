using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class DepartmentConversationType
    {
        public Guid Id { get; set; }

        public string TypeName { get; set; }

        public bool? IsActive { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual ICollection<DepartmentConversation> DepartmentConversations { get; set; } = new List<DepartmentConversation>();
    }
}
