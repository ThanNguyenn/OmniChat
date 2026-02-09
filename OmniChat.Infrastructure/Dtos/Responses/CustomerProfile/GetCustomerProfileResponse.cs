using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.CustomerProfile
{
    public class GetCustomerProfileResponse
    {
        public Guid Id { get; set; }
        
        public string CustomerName { get; set; }
       
        public string? Email { get; set; }
       
        public string? PhoneNumber { get; set; }
       
        public string? AvatarUrl { get; set; }
       
        public string? ZaloSenderId { get; set; }
      
        public string? FacebookSenderId { get; set; }
     
        public string? InstagramSenderId { get; set; }

        public string? CurrentProviderName { get; set; }

        public int? TotalOrder {  get; set; }

        public DateTime?  CustomerDate { get; set; }

        public double? TotalPayment { get; set; }
    }
}
