using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Product;

public class GetAllProductsResponse
{
    public Guid Id { get; set; }

    public string ImageUrl { get; set; }

    public string Name { get; set; }

    public PackagingType ProductPackagingType { get; set; }

    public double VolumeMl { get; set; }
    public ProductKind ProductKind { get; set; }
    public string Description { get; set; }

    public Guid BrandId { get; set; }

    public string Brand { get; set; }

    public double Price { get; set; }

    public string Code { get; set; }

    public int Quantity { get; set; }

    public int LifeSpan { get; set; }

    public DateTime CreateDate { get; set; }
}
