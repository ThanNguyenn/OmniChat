using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Webhooks.Instagram.InstagramMessage
{
    public class InstagramChangeValue
    {
        public InstagramSender Sender { get; set; }
        public InstagramRecipient Recipient { get; set; }
        public string Timestamp { get; set; }
        public InstagramWebhookMessage Message { get; set; }
    }
}
