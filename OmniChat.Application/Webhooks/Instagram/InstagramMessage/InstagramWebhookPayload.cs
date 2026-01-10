using OmniChat.Application.Webhooks.Facebook.WebhookMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Application.Webhooks.Instagram.InstagramMessage
{
    public class InstagramWebhookPayload
    {
        public string Object { get; set; }
        public List<InstagramEntry> Entry { get; set; }
    }
}
