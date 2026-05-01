using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Staff;

public class CreateStaffRequest : IValidatableObject
{
    [Required(ErrorMessage = "Name là bắt buộc")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email là bắt buộc")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Phone là bắt buộc")]
    public string Phone { get; set; }

    public Guid RoleId { get; set; }
        
    public IEnumerable<AssignStaffToIntentTypeRequest>? StaffIntentTypes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RoleId == Guid.Empty)
        {
            yield return new ValidationResult("RoleId là bắt buộc",
                new[] { nameof(RoleId) });
        }
    }
}
