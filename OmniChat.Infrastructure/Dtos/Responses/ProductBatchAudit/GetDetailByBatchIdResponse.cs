using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = OmniChat.Infrastructure.Models.Action;

namespace OmniChat.Infrastructure.Dtos.Responses.ProductBatchAudit;

public class GetDetailByBatchIdResponse
{
    public Guid Id { get; set; }

    public Guid ProductBatchId { get; set; }

    public Guid ProductId { get; set; }
    public string ProductName { get; set; }

    public ProductKind ProductKind { get; set; }

    public string BrandName { get; set; }

    public double VolumeML { get; set; }

    public double Price { get; set; }

    public string ProductCode { get; set; }

    public PackagingType PackagingType { get; set; }

    public string BatchCode { get; set; }

    public DateTime BatchCreateDate {  get; set; }

    public int BatchQuantity { get; set; }

    public DateTime BatchExpiredDate { get; set; }

    public Guid? ActionById { get; set; }

    public string StaffName { get; set; }

    public int OldValue { get; set; }

    public int NewValue { get; set; }

    public Action Action { get; set; }

    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
}
