using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.OrderItem;

public class AddOrderItemRequest : IValidatableObject
{
    public Guid ProductBatchId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity phải >= 1")]
    public int Quantity { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (ProductBatchId == Guid.Empty)
        {
            yield return new ValidationResult("ProductBatchId là bắt buộc",
                new[] { nameof(ProductBatchId) });
        }
    }
}
