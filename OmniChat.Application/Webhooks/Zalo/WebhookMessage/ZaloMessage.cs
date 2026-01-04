using System.Text.Json.Serialization;

namespace OmniChat.Application.Webhooks.Zalo.WebhookMessage
{
    public class ZaloMessage
    {
        [JsonPropertyName("msg_id")]
        public string MsgId { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
