using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class DepartmentConversation
    {
        public Guid Id { get; set; }

        public Guid DepartmentId { get; set; }

        public virtual Department Department { get; set; }

        public string ConversationName { get; set; }

        public bool? IsActive { get; set; }

        public DateTime CreateDate { get; set; }

        public DepartmentConversationStatus Status { get; set; }

        public virtual ICollection<DepartmentStaffMessage> DepartmentStaffMessages { get; set; } = new List<DepartmentStaffMessage>();

        public Guid DepartmentConversationTypeId { get; set; }

        public virtual DepartmentConversationType DepartmentConversationType { get; set; }

        public virtual ICollection<DepartmentConversationFile> DepartmentConversationFiles { get; set; } = new List<DepartmentConversationFile>();

    }

    public enum DepartmentConversationStatus
    {
        Offline = 0,
        Online = 1,
    }
}
