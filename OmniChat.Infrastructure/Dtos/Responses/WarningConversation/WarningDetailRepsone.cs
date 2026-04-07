using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.WarningConversation
{
    public class WarningDetailRepsone
    {
        public string CustomerName { get; set; }

        public string StaffName { get; set; }

        public DateTime CreateAt { get; set; }

        public WarningType WarningType { get; set; }

        public string Reason { get; set; }
    }
}
