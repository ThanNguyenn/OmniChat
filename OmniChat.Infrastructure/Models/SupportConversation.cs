using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class SupportConversation
    {
        public Guid Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public ConversationStatus Status { get; set; }

        public bool IsDistributed { get; set; }

        public string CustomerName { get; set; }

        public string? AvatarUrl { get; set; }

        public Guid? ActiveStaffId { get; set; }

        public virtual Staff? Staff { get; set; }

        public Guid ActiveCustomerId { get; set; }

        public virtual CustomerProfile CustomerProfile { get; set; }

        public Guid ProvidersId { get; set; }

        public virtual Provider Providers { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime FirstResponseAt { get; set; } = DateTime.UtcNow;

        public DateTime CloseAt { get; set; } = DateTime.UtcNow;



        public virtual ICollection<CustomerMessage> CustomerMessages { get; set; } = new List<CustomerMessage>();

        public virtual FeedBack FeedBack { get; set; }
    
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    
        public virtual ICollection<SupportStaffMessage> SupportStaffMessages { get; set; } = new List<SupportStaffMessage>();

        public virtual ICollection<SupportConversationFile> SupportConversationFiles { get; set; } = new List<SupportConversationFile>();

        public virtual ICollection<SupportTask> SupportTasks { get; set; } = new List<SupportTask>();

    }

    public enum ConversationStatus
    {
        Pending = 0,
        Complete = 1,
        Waiting = 2,
        Warning = 3,
    }
}
