using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Order;

public class DraftOrderRequest : IValidatableObject
{
    public required Guid ConversationId { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (ConversationId == Guid.Empty)
        {
            yield return new ValidationResult("ConversationId là bắt buộc",
                new[] { nameof(ConversationId) });
        }
    }
}
