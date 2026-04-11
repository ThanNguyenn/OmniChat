using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Order;

public class DraftOrderItem
{
    public int Quantity { get; set; }
    public string? Volume { get; set; } 
    public string? Unit { get; set; } 
    public string? Brand { get; set; }
    public string? Kind { get; set; }    

    public override string ToString()
    {
        return $"Qty={Quantity}, Vol={Volume}, Unit={Unit}, Brand={Brand}, Kind={Kind}";
    }
}