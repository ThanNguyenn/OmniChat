using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Order;
using OmniChat.Infrastructure.Dtos.Requests.OrderItem;
using OmniChat.Infrastructure.Dtos.Responses.Order;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class DraftOrderService : BaseService<DraftOrderService>, IDraftOrderService
{
    private readonly IOrderService _orderService;
    public DraftOrderService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<DraftOrderService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IOrderService orderService) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _orderService = orderService;
    }

    public async Task<bool> CreateDraftOrderAsync(Guid customerId, string message)
    {
        var parsedItems = Parse(message);

        if (parsedItems == null || parsedItems.Count == 0)
            return false;

        var orderItems = new List<AddOrderItemRequest>();

        foreach (var item in parsedItems)
        {
            var allocations = await ResolveBatchesAsync(item);

            foreach (var (batchId, qty) in allocations)
            {
                orderItems.Add(new AddOrderItemRequest
                {
                    ProductBatchId = batchId,
                    Quantity = qty
                });
            }
        }

        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Name = $"Auto Draft for customer {customerId}",
            OrderItems = orderItems
        };

        return await _orderService.CreateOrderAsync(request);
    }

    private static Regex quantityRegex = new Regex(@"(\d+)\s*(c|chai|ch)\b", RegexOptions.Compiled);
    private static Regex volumeRegex = new Regex(@"(180|190|490|880|1760)\s*(ml)?|nhi|nua lit|1 lit|2 lit", RegexOptions.Compiled);
    private static Regex kindRegex = new Regex(@"(khong duong|ko duong|kd|co duong|duong|sua chua)", RegexOptions.Compiled);
    private static Regex brandRegex = new Regex(@"(long thanh|lothamilk|lt milk|lotha milk)", RegexOptions.Compiled);

    public List<DraftOrderItem> Parse(string raw)
    {
        string text = Normalize(raw);

        var segments = Regex.Split(text, @"[,;]|\band\b")
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .ToList();

        var results = new List<DraftOrderItem>();

        foreach (var seg in segments)
        {
            var item = ParseSegment(seg.Trim());
            if (item != null)
                results.Add(item);
        }

        return results;
    }

    private DraftOrderItem ParseSegment(string text)
    {
        var item = new DraftOrderItem();

        // Volume
        var vMatch = volumeRegex.Match(text);
        if (vMatch.Success)
        {
            item.Volume = NormalizeVolume(vMatch.Value);

            text = text.Replace(vMatch.Value, " ");
        }
        // Quantity
        var qMatch = quantityRegex.Match(text);

        if (qMatch.Success)
        {
            item.Quantity = int.Parse(qMatch.Groups[1].Value);
            item.Unit = NormalizeUnit(qMatch.Groups[2].Value);
        }
        else
        {
            var matches = Regex.Matches(text, @"\b\d+\b");
            if (matches.Count > 0)
            {
                item.Quantity = int.Parse(matches[matches.Count - 1].Value);
                item.Unit = "chai";
            }
        }

        // Kind
        var kMatch = kindRegex.Match(text);
        if (kMatch.Success)
        {
            item.Kind = NormalizeKind(kMatch.Value);
        }

        // Brand
        var bMatch = brandRegex.Match(text);
        if (bMatch.Success)
        {
            item.Brand = NormalizeBrand(bMatch.Value);
        }

        // --- Defaults ---
        if (item.Brand == null)
            item.Brand = "long thanh";

        if (item.Kind == null)
            item.Kind = "sugar";

        if (item.Quantity <= 0 && item.Volume == null)
            return null;

        return item;
    }

    // --- Normalization helpers ---

    private string Normalize(string input)
    {
        string text = input.ToLower();

        // remove accents
        text = RemoveDiacritics(text);

        // spacing fixes
        text = Regex.Replace(text, @"(\d)([a-z])", "$1 $2");
        text = Regex.Replace(text, @"([a-z])(\d)", "$1 $2");

        return text;
    }

    private string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var chars = normalized.Where(c => Char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string(chars.ToArray()).Normalize(NormalizationForm.FormC);
    }

    private string NormalizeVolume(string v)
    {
        v = v.Trim();

        if (v.Contains("180") || v.Contains("190") || v.Contains("nhi") || v.Contains("nho")) return "180";
        if (v.Contains("490") || v.Contains("nua lit")) return "490";
        if (v.Contains("880") || v.Contains("1 lit")) return "880";
        if (v.Contains("1760") || v.Contains("2 lit")) return "1760";

        return null;
    }

    private string NormalizeKind(string k)
    {
        k = k.Trim();

        if (k.Contains("sua chua")) return "yogurt";
        if (k.Contains("kd") || k.Contains("khong duong") || k.Contains("ko duong")) return "no_sugar";
        if (k.Contains("duong") || k.Contains("co duong")) return "sugar";

        return "sugar";
    }

    private string NormalizeBrand(string b)
    {
        b = b.Trim();

        if (b.Contains("lothamilk") || b.Contains("lotha") || b.Contains("lotha milk"))
            return "lothamilk";

        return "long thanh";
    }

    private string NormalizeUnit(string u)
    {
        if (string.IsNullOrEmpty(u)) return "chai";

        if (u == "c" || u == "ch") return "chai";

        return u;
    }

    private async Task<List<(Guid batchId, int quantity)>> ResolveBatchesAsync(DraftOrderItem item)
    {
        var batchRepo = _unitOfWork.GetRepository<ProductBatch>();

        var volume = MapVolume(item.Volume);
        var kind = MapKind(item.Kind);
        var brandId = await MapBrandId(item.Brand);

        var batches = await batchRepo
            .GetQueryable(include: q => q.Include(b => b.Product))
            .Where(b =>
                Math.Abs(b.Product.VolumeMl - volume) < 0.1 &&
                b.Product.ProductKind == kind &&
                b.Product.BrandId == brandId &&
                b.Quantity > 0 &&
                b.IsActive == true)
            .OrderBy(b => b.ManuFactureDate) // FIFO
            .ToListAsync();

        var remaining = item.Quantity;
        var result = new List<(Guid, int)>();

        foreach (var b in batches)
        {
            if (remaining <= 0) break;

            var take = Math.Min(remaining, b.Quantity);

            result.Add((b.Id, take));
            remaining -= take;
        }

        if (remaining > 0)
            throw new BusinessException($"Not enough stock for {item.Volume}-{item.Brand}-{item.Kind}");

        return result;
    }
    private double MapVolume(string v)
    {
        return v switch
        {
            "180" => 180,
            "490" => 490,
            "880" => 880,
            "1760" => 1760,
            _ => throw new BusinessException($"Invalid volume: {v}")
        };
    }
    private ProductKind MapKind(string k)
    {
        return k switch
        {
            "sugar" => ProductKind.Sugar,
            "no_sugar" => ProductKind.NoSugar,
            "yogurt" => ProductKind.Yogurt,
            _ => ProductKind.Sugar
        };
    }
    private async Task<Guid> MapBrandId(string brand)
    {
        var brandRepo = _unitOfWork.GetRepository<Brand>();

        string Normalize(string input)
        {
            input = input.ToLower();
            input = RemoveDiacritics(input);
            input = input.Replace(" ", "");
            return input;
        }

        var normalizedInput = Normalize(brand);

        var brands = await brandRepo.GetQueryable().ToListAsync();

        var entity = brands.FirstOrDefault(b =>
            Normalize(b.Name).Contains(normalizedInput)
        );

        if (entity == null)
            throw new NotFoundException($"Brand not found: {brand}");

        return entity.Id;
    }
}
