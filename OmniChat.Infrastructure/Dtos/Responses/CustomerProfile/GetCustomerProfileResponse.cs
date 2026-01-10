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
        public Guid ProvidersId { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
        public string SenderId { get; set; }
    }
}
