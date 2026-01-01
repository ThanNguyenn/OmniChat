namespace OmniChat.Api.Webhooks.Facebook.WebhookMessage
{
    public class FacebookWebhookMessage
    {
        public string Mid { get; set; }

        public string Text { get; set; }

        public bool? IsEcho { get; set; }
    }
}
