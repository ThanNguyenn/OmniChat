using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class TaskCancelReason
    {
        public Guid Id { get; set; }
        
        public Guid SupportTaskId { get; set; }

        public virtual SupportTask SupportTask { get; set; }

        public ReasonType ReasonType { get; set; }

        public string ? Description { get; set; }

        public Guid CancelledByStaffId { get; set; }

        public DateTime? CreateDate { get; set; } = DateTime.UtcNow;
    }

    public enum ReasonType
    {
       CustomerCancelled = 0,
       WrongAssignment = 1,
    }
}
