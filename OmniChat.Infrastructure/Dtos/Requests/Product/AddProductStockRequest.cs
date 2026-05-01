using OmniChat.Infrastructure.Dtos.Requests.ProductBatch;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Product;

public class AddProductStockRequest : IValidatableObject
{
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "ProductBatch là bắt buộc")]
    public IEnumerable<AddProductBatchRequest> ProductBatch { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (ProductId == Guid.Empty)
        {
            yield return new ValidationResult("OrderItemId là bắt buộc",
                new[] { nameof(ProductId) });
        }
    }

}
