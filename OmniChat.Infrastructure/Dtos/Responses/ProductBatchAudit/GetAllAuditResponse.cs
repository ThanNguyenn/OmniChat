using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.ProductBatchAudit;

public class GetAllAuditResponse
{
    public Guid Id { get; set; }

    public Guid ProductBatchId { get; set; }

    public Guid? ActionById { get; set; }

    public string StaffName { get; set; }

    public int OldValue { get; set; }

    public int NewValue { get; set; }

    public Action Action { get; set; }

    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
}
