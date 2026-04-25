using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Order;

public class DraftOrderContext
{
    public Guid CustomerId { get; set; }
    public List<DraftOrderItem> Items { get; set; } = new();

    public DraftOrderItem LastFocusedItem { get; set; }
}
