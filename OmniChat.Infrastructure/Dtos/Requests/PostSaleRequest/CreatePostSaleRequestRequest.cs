using OmniChat.Infrastructure.Dtos.Requests.PostSaleRequestItem;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.PostSaleRequest;

public class CreatePostSaleRequestRequest
{
    public Guid CustomerId { get; set; }

    public Guid OrderId { get; set; }

    public Guid PresentByStaffId { get; set; }

    public PostSaleRequestType Type { get; set; }

    public string Reason { get; set; }

    public IEnumerable<CreatePostSaleRequestItemRequest>? PostSaleItems { get; set; }
}
