using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Keyword;

public class CreateKeywordRequest : IValidatableObject
{
    public required Guid IntentTypeId { get; set; }

    [Required(ErrorMessage = "KeywordText là bắt buộc")]
    [StringLength(255, ErrorMessage = "KeywordText tối đa 255 ký tự")]
    public string KeywordText { get; set; }

    [Range(0.1f, float.MaxValue, ErrorMessage = "Weight phải >= 0.1")]
    public float Weight { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var scaled = Weight * 10f;

        var rounded = MathF.Round(scaled);
        if (IntentTypeId == Guid.Empty)
        {
            yield return new ValidationResult("IntentTypeId là bắt buộc",
                new[] { nameof(IntentTypeId) });
        }

        if (MathF.Abs(scaled - rounded) > 0.00001f)
        {
            yield return new ValidationResult(
                "Weight chỉ được có 1 chữ số thập phân",
                new[] { nameof(Weight) });
        }
    }
}
