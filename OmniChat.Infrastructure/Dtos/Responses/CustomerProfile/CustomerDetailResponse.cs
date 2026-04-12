using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.CustomerProfile
{
    public class CustomerDetailResponse
    {
        public Guid Id { get; set; }

        public string CustomerName { get; set; }

        public string? CustomerPhone { get; set; }

        public string? Email { get; set; }
    
        public string? Address { get; set; }

        public string? ProviderName { get; set; }

        public DateTime? TimeStartSupport { get; set; }

        public int? TotalOrder {  get; set; } = 0;

        public DateTime? BecomeCustomerDate { get; set; }

        public double? TotalPay {  get; set; }
    }
}
