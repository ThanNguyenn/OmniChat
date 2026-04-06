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

        public int ReassignmentCount { get; set; }

        public int CancelledCount { get; set; }

        public int ConversationOwned { get; set; }

        public int TotalTaskHandleTime { get; set; } //TotalTaskHandleTime += (CompleteDate - CreatedAt).TotalSeconds

        public int TotalFirstResponseTime { get; set; } //TotalFirstResponseTime += (FirstResponseAt - CreatedDate).TotalSeconds

        public double? AvgTaskHandleTime { get; set; }  // TotalTaskHandleTime / TaskCompleted

        public double? AvgFirstResponseTime { get; set; } // AvgFirstResponseTime = TotalFirstResponseTime / ConversationOwned

        public DateTime? FromTime { get; set; }

        public DateTime? ToTime { get; set; }

        public DateTime? CreateDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateDate { get; set; } = DateTime.UtcNow;
    }
}
