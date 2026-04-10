using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.TaskCancelReason
{
    public class TaskCancelReasonRequest
    {
        public Guid SupportTaskId { get; set; }

        public ReasonType ReasonType { get; set; }

        public string? Description { get; set; }

        public Guid CancelledByStaffId { get; set; }
    }
}
