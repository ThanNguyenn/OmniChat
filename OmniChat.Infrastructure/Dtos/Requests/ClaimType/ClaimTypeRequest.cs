using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.ClaimType
{
    public class ClaimTypeRequest
    {
        [Required(ErrorMessage = "TypeName is required")]
        [StringLength(200, ErrorMessage = "TypeName must not exceed 200 characters")]
        [RegularExpression(@"^(?=.*\S)[a-zA-Z0-9 _-]+$",
         ErrorMessage = "TypeName must not contain special characters or be empty")]
        public string TypeName { get; set; }
    }
}
