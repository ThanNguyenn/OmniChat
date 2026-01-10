using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Webhooks.Instagram.InstagramMessage
{
    public class InstagramChangeValue
    {
        public InstagramSender sender { get; set; }
        public InstagramRecipient recipient { get; set; }
        public string timestamp { get; set; }
        public InstagramWebhookMessage message { get; set; }
    }
}
