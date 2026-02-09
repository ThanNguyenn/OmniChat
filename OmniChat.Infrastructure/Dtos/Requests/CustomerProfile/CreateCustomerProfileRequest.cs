using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.CustomerProfile
{
    public class CreateCustomerProfileRequest
    {
        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; }

        public string Address { get; set; }

        [Url]
        public string? AvatarUrl { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? ZaloSenderId { get; set; }
  
        public string? FacebookSenderId { get; set; }

        public string? InstagramSenderId { get; set; }
    }
}
