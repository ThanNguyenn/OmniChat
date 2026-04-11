using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Invoice;

public class DashBoardInvoiceByYearResponse
{
    public string Month { get; set; }

    public double TotalAmount { get; set; }
}
