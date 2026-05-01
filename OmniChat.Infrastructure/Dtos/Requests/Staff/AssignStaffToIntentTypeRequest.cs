using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Staff;

public class AssignStaffToIntentTypeRequest : IValidatableObject
{
    public Guid IntentId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (IntentId == Guid.Empty)
        {
            yield return new ValidationResult("IntentId là bắt buộc",
                new[] { nameof(IntentId) });
        }
    }

}
