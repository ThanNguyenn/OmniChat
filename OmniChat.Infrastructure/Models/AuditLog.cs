using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public string Action { get; set; }

        public Guid? StaffId { get; set; }

       public Guid? EntityId { get; set; }

        public string OldData { get; set; }

        public string NewData { get; set; }

        public DateTime CreateDate { get; set; }

        public string EntityType { get; set; }

    }
}
