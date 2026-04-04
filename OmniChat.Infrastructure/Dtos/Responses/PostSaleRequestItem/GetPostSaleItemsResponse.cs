using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.PostSaleRequestItem;

public class GetPostSaleItemsResponse
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
}
