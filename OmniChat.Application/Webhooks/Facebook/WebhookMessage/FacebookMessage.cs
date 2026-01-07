namespace OmniChat.Application.Webhooks.Facebook.WebhookMessage
{
    public class FacebookMessage
    {
        public FacebookSender sender { get; set; }
        public FacebookRecipient recipient { get; set; }
        public long timestamp { get; set; }
        public FacebookWebhookMessage message { get; set; }
    }
}
