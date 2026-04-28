using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class SheetExportService : BaseService<SheetExportService>, ISheetExportService
{
    public SheetExportService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<SheetExportService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task<(Stream content, string filename)> ExportInvoiceToExcelAsync(Guid invoiceId, string path)
    {
        var invoice = await FetchData(invoiceId);

        var templatePath = path;

        using var workbook = new XLWorkbook(templatePath);
        var ws = workbook.Worksheet(1);

        //title
        ws.Cell("B1").Value = $"{invoice.CustomerProfile.CustomerName} {invoice.StartedDate:dd/MM/yyyy} - {invoice.EndedDate:dd/MM/yyyy}";

        //date
        var dateRowMap = new Dictionary<DateTime, int>();

        int startRow = 4;
        int row = startRow;

        for (var date = invoice.StartedDate.Value.Date; date <= invoice.EndedDate.Value.Date; date = date.AddDays(1))
        {
            ws.Cell(row, 1).Value = date;
            ws.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy";

            dateRowMap[date] = row;
            row++;
        }
        //products
        var productMap = GetProductMap();

        var returnedQtyByOrderItem = invoice.Orders
            .SelectMany(o => o.PostSaleRequests)
            .SelectMany(ps => ps.PostSaleItems)
            .GroupBy(pi => pi.OrderItemId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        foreach (var order in invoice.Orders)
        {
            if (!order.OrderDate.HasValue)
                continue;

            var date = order.OrderDate.Value.Date;

            if (!dateRowMap.TryGetValue(date, out int excelRow))
                continue;

            foreach (var item in order.OrderItems)
            {
                var product = item.ProductBatch.Product;

                var key = (
                    brand: product.Brand.Name,
                    volume: product.VolumeMl,
                    kind: product.ProductKind
                );

                if (!productMap.TryGetValue(key, out int col))
                    continue;

                var cell = ws.Cell(excelRow, col);

                int current = 0;
                if (!cell.IsEmpty())
                    current = cell.GetValue<int>();

                returnedQtyByOrderItem.TryGetValue(item.Id, out var returnedQty);

                var effectiveQty = item.Quantity - returnedQty;

                if (effectiveQty <= 0)
                    continue;

                cell.Value = current + effectiveQty;
            }
        }

        //debt
        var debt = await CalculateDebt(invoice.Id, invoice.CustomerId);
        ws.Cell("AB12").Value = debt;

        //deducted amount
        ws.Cell("AB13").Value = invoice.DeductedAmount;


        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return (stream, $"{invoice.CustomerProfile.CustomerName}_{invoice.StartedDate:dd/MM/yyyy}-{invoice.EndedDate:dd/MM/yyyy}_invoice.xlsx");
    }

    private async Task<Invoice> FetchData(Guid invoiceId)
    {
        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();
        var invoice = await invoiceRepo.GetQueryable(
            predicate: i => i.Id == invoiceId, 
            include: q => q
                .Include(i => i.CustomerProfile)

                .Include(i => i.Orders)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductBatch)
                            .ThenInclude(pb => pb.Product)
                                .ThenInclude(p => p.Brand)

                .Include(i => i.Orders)
                    .ThenInclude(o => o.PostSaleRequests
                        .Where(ps => ps.Status == PostSaleRequestStatus.Approved &&
                                     ps.Type == PostSaleRequestType.Return))
                        .ThenInclude(ps => ps.PostSaleItems),
            asNoTracking: true).AsSplitQuery().FirstOrDefaultAsync() ?? throw new NotFoundException($"Không tìm thấy phiếu thanh toán");
        return invoice;
    }

    private async Task<double> CalculateDebt(Guid currentId, Guid customerId)
    {
        var invoiceRepo = _unitOfWork.GetRepository<Invoice>();
        var invoices = await invoiceRepo.GetListAsync(
            predicate: i =>
                i.Id != currentId &&
                !new[]
                {
                    InvoiceStatus.Cancel,
                    InvoiceStatus.Completed,
                    InvoiceStatus.Refunded,
                    InvoiceStatus.PendingRefund
                }.Contains(i.InvoiceStatus)
        );

        double totalDebt = 0.0;

        foreach (var item in invoices)
        {
            var remaining = (item.Total - item.DeductedAmount) - item.PaidAmount;

            if (remaining > 0)
                totalDebt += remaining;
        }

        return totalDebt;
    }

    //Map product 
    private static Dictionary<(string brand, double volume, ProductKind kind), int> GetProductMap()
    {
        const string LONG_THANH = "Long Thành";
        const string LOTHAMILK = "Lothamilk";
        return new Dictionary<(string brand, double volume, ProductKind kind), int>
        {
            // LONG THÀNH
            { (LONG_THANH, 180, ProductKind.Sugar), 2 },   // B
            { (LONG_THANH, 490, ProductKind.Sugar), 3 },   // C
            { (LONG_THANH, 880, ProductKind.Sugar), 4 },   // D
            { (LONG_THANH, 1760, ProductKind.Sugar), 5 },  // E

            { (LONG_THANH, 180, ProductKind.NoSugar), 6 }, // F
            { (LONG_THANH, 490, ProductKind.NoSugar), 7 }, // G
            { (LONG_THANH, 880, ProductKind.NoSugar), 8 }, // H
            { (LONG_THANH, 1760, ProductKind.NoSugar), 9 },// I

            { (LONG_THANH, 180, ProductKind.Yogurt), 10 }, // J
            { (LONG_THANH, 490, ProductKind.Yogurt), 11 }, // K
            { (LONG_THANH, 880, ProductKind.Yogurt), 12 }, // L
            { (LONG_THANH, 1760, ProductKind.Yogurt), 13 },// M

            // LOTHAMILK
            { (LOTHAMILK, 180, ProductKind.Sugar), 14 },   // N
            { (LOTHAMILK, 490, ProductKind.Sugar), 15 },   // O
            { (LOTHAMILK, 880, ProductKind.Sugar), 16 },   // P
            { (LOTHAMILK, 1760, ProductKind.Sugar), 17 },  // Q

            { (LOTHAMILK, 180, ProductKind.NoSugar), 18 }, // R
            { (LOTHAMILK, 490, ProductKind.NoSugar), 19 }, // S
            { (LOTHAMILK, 880, ProductKind.NoSugar), 20 }, // T
            { (LOTHAMILK, 1760, ProductKind.NoSugar), 21 },// U

            { (LOTHAMILK, 180, ProductKind.Yogurt), 22 },  // V
            { (LOTHAMILK, 490, ProductKind.Yogurt), 23 },  // W
            { (LOTHAMILK, 880, ProductKind.Yogurt), 24 },  // X
            { (LOTHAMILK, 1760, ProductKind.Yogurt), 25 }, // Y
        };
    }
}
