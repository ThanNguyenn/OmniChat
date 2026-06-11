using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Invoice
{
    public class InvoiceHistoriesResponse
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }

        public DateTime? StartedDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndedDate { get; set; }

        public double Total { get; set; }

        public InvoiceStatus InvoiceStatus { get; set; }

        public InvoiceMethod InvoiceMethod { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime CreateAt { get; set; }

        public bool? IsDeleted { get; set; }

        public double PaidAmount { get; set; }

        //public double DeductedAmount { get; set; }

        public long InvoiceCode { get; set; }
    }
}
