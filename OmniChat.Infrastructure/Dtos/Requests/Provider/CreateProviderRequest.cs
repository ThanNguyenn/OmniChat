using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Provider
{
    public class CreateProviderRequest
    {
        [Required(ErrorMessage = "ProviderName is required")]
        [StringLength(200, ErrorMessage = "ProviderName must not exceed 200 characters")]
        [RegularExpression(@"^(?=.*\S)[a-zA-Z0-9 _-]+$",
        ErrorMessage = "ProviderName must not contain special characters or be empty")]
        public string ProviderName { get; set; }
    }
}
