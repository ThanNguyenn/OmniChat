namespace OmniChat.Application.Webhooks.Facebook.WebhookMessage
{
    public class FacebookWebhookMessage
    {
        public string mid { get; set; }

        public string text { get; set; }

        public bool? is_echo { get; set; }
    }
}
