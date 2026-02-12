using OmniChat.Infrastructure.Dtos.Requests.ProductBatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Product;

public class AddProductStockRequest
{
    public Guid ProductId { get; set; }
    public IEnumerable<AddProductBatchRequest> ProductBatch { get; set; }

}
