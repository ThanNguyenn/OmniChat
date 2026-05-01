using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Keyword;

public class UpdateKeywordRequest : IValidatableObject
{
    [Range(0.1f, float.MaxValue, ErrorMessage = "Weight phải >= 0.1")]
    public float Weight { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        var scaled = Weight * 10;
        if (Math.Abs(scaled - MathF.Round(scaled)) > 0.0001f)
        {
            yield return new ValidationResult(
                "Weight chỉ được có 1 chữ số thập phân",
                new[] { nameof(Weight) });
        }
    }
}
