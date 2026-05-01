using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Wallet;

public class WalletPaymentRequest : IValidatableObject
{
    public Guid CustomerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Amount phải >= 1")]
    public int Amount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (CustomerId == Guid.Empty)
        {
            yield return new ValidationResult("CustomerId là bắt buộc",
                new[] { nameof(CustomerId) });
        }
    }
}
