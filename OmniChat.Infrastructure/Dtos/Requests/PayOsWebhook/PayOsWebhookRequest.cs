using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.PayOsWebhook
{
    public class PayOsWebhookRequest
    {
        public string code { get; set; }    
        public string desc { get; set; }     
        public bool success { get; set; }
        public WebhookData data { get; set; }
        public string signature { get; set; }
    }
}
