using OmniChat.Application.Webhooks.Facebook.WebhookMessage;
using OmniChat.Application.Webhooks.Instagram.InstagramMessage;
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
        public Task<bool> ZaloWebhookAsync(ZaloWebhookEvent zaloEvent);

        public  Task<bool> FacebookWebhookAsync(FaceBookWebhookPayload faceBookWebhookPayload);

        public Task<bool> VerifyFacebookWebhook(string mode, string token);

        public  Task<bool> InstagramWebhookAsync(InstagramWebhookPayload payload);

        public  Task<bool> VerifyInstagramWebhook(string mode, string token);
    }
}
