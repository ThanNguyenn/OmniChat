namespace OmniChat.Api.Webhooks.Facebook.WebhookMessage
{
    public class FacebookMessage
    {
        public FacebookSender Sender { get; set; }

        public FacebookRecipient Recipient { get; set; }

        public long Timestamp { get; set; }

        public FacebookWebhookMessage Message { get; set; }
    }
}
