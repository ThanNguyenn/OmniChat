using System.Text.Json.Serialization;

namespace OmniChat.Application.Webhooks.Zalo.WebhookMessage
{
    public class ZaloWebhookEvent
    {
        [JsonPropertyName("event_name")]
        public string EventName { get; set; }

        [JsonPropertyName("sender")]
        public ZaloSender Sender { get; set; }

        [JsonPropertyName("recipient")]
        public ZaloRecipient Recipient { get; set; }

        [JsonPropertyName("message")]
        public ZaloMessage Message { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }
}
