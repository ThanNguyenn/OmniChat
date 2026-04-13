using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Product;

public class GetAllProductsCreateOrderResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public PackagingType ProductPackagingType { get; set; }

    public double VolumeMl { get; set; }
    public ProductKind ProductKind { get; set; }
    public Guid BrandId { get; set; }
    public string Brand { get; set; }

    public string Code { get; set; }

    public int Quantity { get; set; }
}
