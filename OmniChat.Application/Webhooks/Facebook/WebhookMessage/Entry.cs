namespace OmniChat.Application.Webhooks.Facebook.WebhookMessage
{
    public class Entry
    {
        public string Id { get; set; }

        public long Time { get; set; }

       public List<FacebookMessage> facebookMessages = new List<FacebookMessage>();
    }
}
