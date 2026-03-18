using Microsoft.AspNetCore.Http;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Product;

public class CreateProductRequest
{
    public string Name { get; set; }

    public PackagingType ProductPackagingType { get; set; }

    public double VolumeMl { get; set; }

    public string Description { get; set; }

    public Guid BrandId { get; set; }

    public double Price { get; set; }

    public int LifeSpan { get; set; }

    public IFormFile? Image { get; set; }
}

