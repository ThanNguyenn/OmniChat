using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class CustomerProfile
    {
        public Guid Id { get; set; }

        public string? CustomerName { get; set; }
        
        public string? Address { get; set; }

        public string? AvatarUrl { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? ZaloSenderId { get; set; }

        public string? FacebookSenderId { get; set; }

        public string? InstagramSenderId { get; set; }

        public bool? IsNewCustomer { get; set; }

        public bool? IsBlocked { get; set; } 

        public DateTime? CreateDate { get; set; } = DateTime.UtcNow;

        public bool IsFormSent { get; set; } = false; // Indicates whether the form has been sent to the customer

        public bool IsProfileCompleted { get; set; } = false; // flag to make sure customer no send multiple form with same customer profile
       
        public double DebtLimit { get; set; }

        public virtual ICollection<CustomerMessage> CustomerMessages { get; set; } = new List<CustomerMessage>();

        public virtual ICollection<SupportConversation> SupportConversations { get; set; } = new List<SupportConversation>();

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        public virtual ICollection<PostSaleRequest> PostSaleRequests { get; set; } = new List<PostSaleRequest>();

        public virtual Wallet? Wallet { get; set; }
    }
}
