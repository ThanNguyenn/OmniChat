using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.SupportStaffMessage
{
    public class CreateSupportStaffMessageResponse
    {
        public Guid Id { get; set; }

        public Guid SupportConversationId { get; set; }

        public Guid StaffId { get; set; }

        public string Content { get; set; }

        public SupportStaffMessageStatus Status { get; set; } // to conversation still pending , after send to facebook or zalo success will become sent

        public long Timestamp { get; set; }
    }
}
