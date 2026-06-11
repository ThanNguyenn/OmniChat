using OmniChat.Infrastructure.Dtos.Responses.Invoice;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Order
{
    public class InvoiceOrderResponse
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }

        public Guid? InvoiceId { get; set; }

        public string Name { get; set; }

        public DateTime? OrderDate { get; set; }

        public OrderStatus Status { get; set; }

        public double TotalAmount { get; set; }

        public DeliveryStatus? DeliveryStatus { get; set; }

        public string Code { get; set; }

        public Guid? DriverId { get; set; }

        public DateTime? DeliveriedDate { get; set; }

        public List<InvoiceItemResponse> OrderItems { get; set; }
    }
}
