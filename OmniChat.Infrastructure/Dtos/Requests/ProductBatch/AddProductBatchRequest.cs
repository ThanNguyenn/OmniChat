using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.ProductBatch;

public class AddProductBatchRequest
{
    public DateTime? ManuFactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int Quantity { get; set; }
}
