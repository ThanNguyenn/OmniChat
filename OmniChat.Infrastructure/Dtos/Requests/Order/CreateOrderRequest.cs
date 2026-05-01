using OmniChat.Infrastructure.Dtos.Requests.OrderItem;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Order;

public class CreateOrderRequest : IValidatableObject
{
    public  Guid CustomerId { get; set; }

    public string Name { get; set; }

    [Required(ErrorMessage = "OrderItems là bắt buộc")]
    public IEnumerable<AddOrderItemRequest> OrderItems { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (CustomerId == Guid.Empty)
        {
            yield return new ValidationResult("CustomerId là bắt buộc",
                new[] { nameof(CustomerId) });
        }
    }
}
