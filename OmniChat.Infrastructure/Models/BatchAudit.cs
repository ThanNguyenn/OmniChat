using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class BatchAudit
    {
        public Guid Id { get; set; }
        
        public Guid ProductBatchId { get; set; }

        public virtual ProductBatch ProductBatch { get; set; }

        public Guid? ActionById { get; set; }

        public virtual Staff ActionBy { get; set; }

        public int OldValue { get; set; }

        public int NewValue { get; set; }

        public Action Action { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }

    public enum Action
    {
        Enter = 0,
        Export = 1,
        Remove = 2,
    }
}
