using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Order;

public class DraftOrderRequest : IValidatableObject
{
    public Guid ConversationId { get; set; }
    [JsonIgnore]
    public Guid CustomerId { get; set; }
    [JsonIgnore]
    //[Required (ErrorMessage = "Messages là bắt buộc")]
    public IEnumerable<string>? Messages { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (ConversationId == Guid.Empty)
        {
            yield return new ValidationResult("ConversationId là bắt buộc",
                new[] { nameof(ConversationId) });
        }
    }

    //public IEnumerable<ValidationResult> Validate(ValidationContext context)
    //{
    //    if (CustomerId == Guid.Empty)
    //    {
    //        yield return new ValidationResult("CustomerId là bắt buộc",
    //            new[] { nameof(CustomerId) });
    //    }
    //}
}
