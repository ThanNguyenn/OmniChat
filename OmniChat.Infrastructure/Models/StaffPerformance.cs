using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class StaffPerformance
    {
        public Guid Id { get; set; }

        public Guid StaffId { get; set; }

        public virtual Staff Staff { get; set; }

        public int TaskCompleted { get; set; }

        public int AvgTaskHandleTime { get; set; }

        public int ConversationOwned { get; set; }

        public int AvgFirstResponseTime { get; set; }

        public int ReassignmentCount { get; set; }

        public int CancelledCount { get; set; }

        public DateTime? FromTime { get; set; } = DateTime.UtcNow;

        public DateTime? ToTime { get; set; } = DateTime.UtcNow;

        public DateTime? CreateDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateDate { get; set; } = DateTime.UtcNow;
    }
}
