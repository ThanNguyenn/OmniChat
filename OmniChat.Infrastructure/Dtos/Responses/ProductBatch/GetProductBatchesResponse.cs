using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.ProductBatch;

public class GetProductBatchesResponse
{
    public Guid Id { get; set; }

    public DateTime ManuFactureDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public int Quantity { get; set; }
}
