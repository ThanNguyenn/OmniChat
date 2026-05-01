using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.PostSaleRequestItem;

public class CreatePostSaleRequestItemRequest : IValidatableObject
{
    public Guid OrderItemId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity phải >= 1")]
    public int Quantity { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (OrderItemId == Guid.Empty)
        {
            yield return new ValidationResult("OrderItemId là bắt buộc",
                new[] { nameof(OrderItemId) });
        }
    }
}
