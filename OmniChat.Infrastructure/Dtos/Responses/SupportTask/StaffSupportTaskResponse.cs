using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.SupportTask
{
    public class StaffSupportTaskResponse
    {
        public string IntentTypeName { get; set; }
        
        public string CustomerName { get; set; }

        public DateTime CompletedAt { get; set; }

    }
}
