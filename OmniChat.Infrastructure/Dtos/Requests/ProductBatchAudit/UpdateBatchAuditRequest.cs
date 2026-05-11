using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.ProductBatchAudit;

public class UpdateBatchAuditRequest
{
        public int? OldValue { get; set; }

        public int? NewValue { get; set; }

        public Models.Action? Action { get; set; }
}
