using System.Text.Json.Serialization;

namespace OmniChat.Application.Webhooks.Zalo.WebhookMessage
{
    public class ZaloRecipient
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}
