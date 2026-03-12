using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Staff
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public StaffStatus Status { get; set; }

        public bool? IsActive { get; set; }

        public Guid? AccountId { get; set; }

        public virtual Account Account { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        public virtual ICollection<StaffIntentType> StaffIntentTypes { get; set; } = new List<StaffIntentType>();

        public virtual ICollection<SupportConversation> SupportConversations { get; set; } = new List<SupportConversation>();

        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

        public virtual ICollection<FeedBack> FeedBacks { get; set; } = new List<FeedBack>();

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    
        public virtual ICollection<SupportStaffMessage> SupportStaffMessages { get; set; } = new List<SupportStaffMessage>();

        public virtual ICollection<InternalStaffMessage> InternalStaffMessages { get; set; } = new List<InternalStaffMessage>();

        public virtual ICollection<SupportTask> SupportTasks { get; set; } = new List<SupportTask>();

        public virtual ICollection<TaskAction> ActionsPerformed { get; set; } = new List<TaskAction>();

        public virtual ICollection<TaskAction> ActionsReceived { get; set; } = new List<TaskAction>();

        public virtual ICollection<PostSaleRequest> PostSaleRequestsPresented { get; set; } = new List<PostSaleRequest>();

        public virtual ICollection<PostSaleRequest> PostSaleRequestsResolved { get; set; } = new List<PostSaleRequest>();

        public virtual ICollection<StaffPerformance> StaffPerformances { get; set; } = new List<StaffPerformance>();
    }

    public enum StaffStatus
    {
        Online = 0,
        Offline = 1,
    }
}
