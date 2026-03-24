using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Product;

public class GetAllProductsCreateOrderQueryRequest
{
    public PackagingType? PackagingType { get; set; }

    public ProductKind? ProductKind { get; set; }

    public double? VolumeMl { get; set; }

    public Guid? BrandId { get; set; }
}
