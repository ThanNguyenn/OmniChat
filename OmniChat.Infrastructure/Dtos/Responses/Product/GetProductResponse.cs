using OmniChat.Infrastructure.Dtos.Responses.ProductBatch;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Product;

public class GetProductResponse
{
    public Guid Id { get; set; }

    public string ImageUrl { get; set; }

    public string Name { get; set; }

    public PackagingType ProductPackagingType { get; set; }

    public double VolumeMl { get; set; }

    public string Description { get; set; }

    public string Brand { get; set; }

    public double Price { get; set; }

    public string Code { get; set; }

    public int Quantity { get; set; }

    public int LifeSpan { get; set; }

    public IEnumerable<GetProductBatchesResponse> ProductBatches { get; set; }
}
