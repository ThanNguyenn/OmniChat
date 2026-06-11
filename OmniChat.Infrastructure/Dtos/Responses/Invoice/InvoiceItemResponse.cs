using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Invoice
{
    public class InvoiceItemResponse
    {
        public string ProductName { get; set; }

        public string ImageUrl { get; set; }

        public int Quantity { get; set; }

        public double SinglePrice { get; set; }

        public double TotalPrice { get; set; }
    }
}
