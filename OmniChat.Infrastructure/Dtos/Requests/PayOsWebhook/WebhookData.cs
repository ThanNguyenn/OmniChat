using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.PayOsWebhook
{
    public class WebhookData
    {
        public long orderCode { get; set; }  
        public int amount { get; set; }   
        public string description { get; set; }
        public string paymentLinkId { get; set; }
        public string reference { get; set; }
    }
}
