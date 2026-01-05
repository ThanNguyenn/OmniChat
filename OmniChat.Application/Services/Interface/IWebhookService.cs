using OmniChat.Application.Webhooks.Facebook.WebhookMessage;
using OmniChat.Application.Webhooks.Zalo.WebhookMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IWebhookService
    {
        public  Task<bool> ZaloWebhookAsync(ZaloWebhookEvent zaloEvent);

        public  Task<bool> FacebookWebhookAsync(FaceBookWebhookPayload faceBookWebhookPayload);
    }
}
