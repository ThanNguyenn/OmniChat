using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.CustomerProfile
{
    public class UpdateCustomerProfileRequest
    {
        public string? CustomerName { get; set; }

        public string? Address { get; set; }

        public string? AvatarUrl { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
    }
}
