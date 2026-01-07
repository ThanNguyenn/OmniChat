using System.Text.Json.Serialization;

namespace OmniChat.Application.Webhooks.Zalo.WebhookMessage
{
    public class ZaloRecipient
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }
    }
}
