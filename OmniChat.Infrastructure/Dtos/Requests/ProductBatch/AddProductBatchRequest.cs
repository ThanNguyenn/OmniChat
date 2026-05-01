using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.ProductBatch;

public class AddProductBatchRequest : IValidatableObject
{
    public DateTime? ManuFactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity phải >= 1")]
    public int Quantity { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!ManuFactureDate.HasValue && !ExpiryDate.HasValue)
        {
            yield return new ValidationResult(
                "Phải nhập ít nhất một trong hai trường: ManuFactureDate hoặc ExpiryDate.",
                new[] { nameof(ManuFactureDate), nameof(ExpiryDate) });
        }
    }
}
