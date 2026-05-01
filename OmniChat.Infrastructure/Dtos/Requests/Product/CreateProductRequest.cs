using Microsoft.AspNetCore.Http;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Product;

public class CreateProductRequest : IValidatableObject
{
    [Required(ErrorMessage = "Name là bắt buộc")]
    public string Name { get; set; }

    public PackagingType ProductPackagingType { get; set; }

    public ProductKind ProductKind { get; set; }

    [Range(1d, double.MaxValue, ErrorMessage = "VolumeMl phải >= 1")]
    public double VolumeMl { get; set; }

    public string Description { get; set; }

    public Guid BrandId { get; set; }

    [Range(1d, double.MaxValue, ErrorMessage = "Price phải >= 1")]
    public double Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "LifeSpan phải >= 1")]
    public int LifeSpan { get; set; }

    public IFormFile? Image { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (BrandId == Guid.Empty)
        {
            yield return new ValidationResult("BrandId là bắt buộc",
                new[] { nameof(BrandId) });
        }
    }
}

