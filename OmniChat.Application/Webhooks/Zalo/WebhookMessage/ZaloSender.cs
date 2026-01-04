using System.Text.Json.Serialization;

namespace OmniChat.Application.Webhooks.Zalo.WebhookMessage
{
    public class ZaloSender
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}
