namespace OmniChat.Application.Webhooks.Facebook.WebhookMessage
{
    public class Entry
    {
        public string Id { get; set; }

        public long Time { get; set; }

        ICollection<FacebookMessage> facebookMessages = new List<FacebookMessage>();
    }
}
