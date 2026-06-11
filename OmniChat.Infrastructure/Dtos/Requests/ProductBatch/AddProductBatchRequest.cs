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
                "Phải nhập cả ngày sản xuất (ManuFactureDate) và ngày hết hạn (ExpiryDate).",
                new[] { nameof(ManuFactureDate), nameof(ExpiryDate) });
        }

        if (ExpiryDate.HasValue && ExpiryDate.Value.Date < DateTime.Today)
        {
            yield return new ValidationResult(
                "Hạn sử dụng (ExpiryDate) không được là một ngày trong quá khứ.",
                new[] { nameof(ExpiryDate) });
        }

        if (ManuFactureDate.HasValue && ExpiryDate.HasValue && ExpiryDate.Value <= ManuFactureDate.Value)
        {
            yield return new ValidationResult(
                "Hạn sử dụng (ExpiryDate) phải lớn hơn Ngày sản xuất (ManuFactureDate).",
                new[] { nameof(ExpiryDate), nameof(ManuFactureDate) });
        }
    }
}
