using OmniChat.Infrastructure.Dtos.Requests.PostSaleRequestItem;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.PostSaleRequest;

public class CreatePostSaleRequestRequest : IValidatableObject
{
    public Guid CustomerId { get; set; }

    public Guid OrderId { get; set; }

    //public Guid PresentByStaffId { get; set; }

    public PostSaleRequestType Type { get; set; }

    public string? Reason { get; set; }

    [Required(ErrorMessage = "PostSaleItems là bắt buộc")]
    public IEnumerable<CreatePostSaleRequestItemRequest>? PostSaleItems { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

        if (CustomerId == Guid.Empty)
        {
            yield return new ValidationResult("CustomerId là bắt buộc",
                new[] { nameof(CustomerId) });
        }

        //if (PresentByStaffId == Guid.Empty)
        //{
        //    yield return new ValidationResult("PresentByStaffId là bắt buộc",
        //        new[] { nameof(PresentByStaffId) });
        //}

        if (OrderId == Guid.Empty)
        {
            yield return new ValidationResult("OrderId là bắt buộc",
                new[] { nameof(OrderId) });
        }
    }   

}
