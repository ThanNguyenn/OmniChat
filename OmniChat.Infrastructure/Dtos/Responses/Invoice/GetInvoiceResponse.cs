using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Invoice;

public class GetInvoiceResponse
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; }

    public string CustomerPhoneNumber { get; set; }

    public string CustomerEmail { get; set; }

    public string CustomerAddress { get; set; }

    public DateTime? StartedDate { get; set; }

    public DateTime? EndedDate { get; set; }

    public double Total { get; set; }

    public InvoiceStatus InvoiceStatus { get; set; }

    public InvoiceMethod InvoiceMethod { get; set; }

    public DateTime? CompletedDate { get; set; }

    public double PaidAmount { get; set; }

    public double DeductedAmount { get; set; }
}
