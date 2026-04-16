using OmniChat.Infrastructure.Dtos.Responses.PostSaleRequestItem;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.PostSaleRequest;

public class GetPostSaleRequestsResponse
{
    public Guid Id { get; set; }
    public string CustomerName{ get; set; } 
    public string PresentByStaffName { get; set; } 
    public PostSaleRequestType Type { get; set; }
    public PostSaleRequestStatus Status { get; set; }
    public string Reason { get; set; }
    public double? RefundAmount { get; set; }
    public DateTime? RequestedTime { get; set; }

    public Guid OrderId { get; set; }

    //List san pham
    public IEnumerable<GetPostSaleItemsResponse> PostSaleItems { get; set; }
}
