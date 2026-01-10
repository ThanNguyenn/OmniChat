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

        [Required]
        public Guid ProvidersId { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        //true => Male
        public bool Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Url]
        public string? AvatarUrl { get; set; }

        [Required]
        public string SenderId { get; set; }
    }
}
