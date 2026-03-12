using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class TaskAction
    {
        public Guid Id { get; set; }
        
        public Guid SupportTaskId { get; set; }

        public virtual SupportTask SupportTask { get; set; }
        
        public TaskActionType Action { get; set; }

        public string Reason { get; set; }

        public DateTime CreateDate { get; set; }

        public Guid ActionById { get; set; }

        public virtual Staff ActionBy { get; set; }

        public Guid ActionToId { get; set; }

        public virtual Staff ActionTo { get; set; }
    }

    public enum TaskActionType
    {
        Assigned = 0,
        Reassigned = 1,
        unassigned = 2,
        Completed = 3,
        Cancelled = 4,
    }


}
