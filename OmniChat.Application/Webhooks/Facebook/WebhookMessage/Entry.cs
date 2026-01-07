namespace OmniChat.Application.Webhooks.Facebook.WebhookMessage
{
    public class Entry
    {
        public string id { get; set; }

        public long time { get; set; }

       public List<FacebookMessage> messaging = new List<FacebookMessage>();
    }
}
