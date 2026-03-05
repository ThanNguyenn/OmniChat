using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class TaskAssignmentHistory
    {
        public Guid Id { get; set; }
        
        public Guid SupportTaskId { get; set; }

        public virtual SupportTask SupportTask { get; set; }
        
        public TaskAssignmentType Action { get; set; }

        public string Reason { get; set; }

        public DateTime CreateDate { get; set; }

        public Guid ActionById { get; set; }

        public virtual Staff ActionBy { get; set; }

        public Guid ActionToId { get; set; }

        public virtual Staff ActionTo { get; set; }
    }

    public enum TaskAssignmentType
    {
        Assigned = 0,
        Reassigned = 1,
        unassigned = 2,

    }
}
