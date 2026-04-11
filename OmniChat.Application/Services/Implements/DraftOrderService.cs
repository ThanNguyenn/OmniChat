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
            try
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
            catch (BusinessException)
            {
                continue;
            }
        }
        if (orderItems.Count == 0)
            return false;
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Name = $"Auto Draft for customer {customerId}",
            OrderItems = orderItems
        };

        return await _orderService.CreateOrderAsync(request);
    }

    private static Regex quantityRegex = new Regex(@"(\d+)\s*(chai|c|ch)\b", RegexOptions.Compiled);
    private static Regex volumeRegex = new Regex(@"(180|190|490|880|1760)\s*(ml)?|nhi|nua lit|1 lit|2 lit", RegexOptions.Compiled);
    private static Regex kindRegex = new Regex(@"(khong duong|ko duong|kd|co duong|duong|sua chua)", RegexOptions.Compiled);
    private static Regex brandRegex = new Regex(@"(long thanh|lothamilk|lt milk|lotha milk)", RegexOptions.Compiled);
    public List<DraftOrderItem> Parse(string raw)
    {
        var text = Normalize(raw);

        text = ExpandAllFormats(text);

        var segments = SplitSegments(text);

        var results = new List<DraftOrderItem>();

        foreach (var seg in segments)
        {
            var items = ParseSegment(seg);
            results.AddRange(items);
        }

        return results;
    }
    private List<DraftOrderItem> ParseSegment(string segment)
    {
        var results = new List<DraftOrderItem>();

        var matches = Regex.Matches(segment,
            @"(?:
            (?<qty>\d+)\s*(chai|c|ch)?\s*(?<vol>180|190|490|880|1760)\s*ml? |
            (?<vol2>180|190|490|880|1760)\s*ml?\s*(?<qty2>\d+) |
            (?<vol3>180|190|490|880|1760)\s+(?<qty3>\d+)\s*(c|chai|ch)
        )",
            RegexOptions.IgnorePatternWhitespace);

        foreach (Match m in matches)
        {
            string vol =
                m.Groups["vol"].Success ? m.Groups["vol"].Value :
                m.Groups["vol2"].Success ? m.Groups["vol2"].Value :
                m.Groups["vol3"].Value;

            int qty =
                m.Groups["qty"].Success ? int.Parse(m.Groups["qty"].Value) :
                m.Groups["qty2"].Success ? int.Parse(m.Groups["qty2"].Value) :
                int.Parse(m.Groups["qty3"].Value);

            var window = segment.Substring(m.Index, Math.Min(30, segment.Length - m.Index));

            var item = new DraftOrderItem
            {
                Quantity = qty,
                Volume = NormalizeVolume(vol),
                Brand = "long thanh",
                Kind = DetectKind(window)
            };

            results.Add(item);
        }

        return results;
    }

    private string DetectKind(string text)
    {
        if (Regex.IsMatch(text, @"(sua chua|^y\b)"))
            return "yogurt";

        if (Regex.IsMatch(text, @"(kd|khong duong|k duong)"))
            return "no_sugar";

        if (Regex.IsMatch(text, @"duong"))
            return "sugar";

        return "sugar";
    }
   

    private string DetectBrand(string text)
    {
        var match = brandRegex.Match(text);

        if (!match.Success)
            return null;

        return NormalizeBrand(match.Value);
    }

    private List<string> SplitSegments(string input)
    {
        var text = Regex.Replace(input, @"\s+", " ");

        return Regex.Split(text,
            @"(?=(\d+\s*(chai|c|ch)?\s*(180|190|490|880|1760)))")
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }
    private string ExpandAllFormats(string input)
    {
        input = ExpandDotFormat(input);
        input = ExpandColonFormat(input);
        input = ExpandInlineCompact(input);
        return input;
    }
    private string ExpandInlineCompact(string text)
    {
        return Regex.Replace(text,
            @"(\d{3,4})\s+((\d+\s*c.*?)+)",
            m =>
            {
                var volume = m.Groups[1].Value;
                var rest = m.Groups[2].Value;

                var matches = Regex.Matches(rest, @"(\d+)\s*c(?:\s*(kd|k duong|duong))?");

                var results = new List<string>();

                foreach (Match x in matches)
                {
                    var qty = x.Groups[1].Value;
                    var kindRaw = x.Groups[2].Value;

                    var kind = Regex.IsMatch(kindRaw, @"kd|k duong")
                        ? "kd"
                        : "duong";

                    results.Add($"{qty} chai {volume}ml {kind}");
                }

                return string.Join(" ", results);
            });
    }
    private string ExpandColonFormat(string text)
    {
        return Regex.Replace(text,
            @"(st|sc)(\d{3,4})\s*:\s*([^\n]+)",
            m =>
            {
                var prefix = m.Groups[1].Value;
                var volume = m.Groups[2].Value;
                var parts = m.Groups[3].Value.Split(',');

                var results = new List<string>();

                foreach (var p in parts)
                {
                    var qtyMatch = Regex.Match(p, @"\d+");
                    if (!qtyMatch.Success) continue;

                    var qty = qtyMatch.Value;

                    if (prefix == "sc")
                    {
                        results.Add($"{qty} chai {volume}ml sua chua");
                    }
                    else
                    {
                        var kind = Regex.IsMatch(p, @"(kd|it duong|it dg)")
                            ? "kd"
                            : "duong";

                        results.Add($"{qty} chai {volume}ml {kind}");
                    }
                }

                return string.Join(" ", results);
            });
    }
    private string ExpandDotFormat(string text)
    {
        return Regex.Replace(text,
            @"(\d{3,4})ml\.\.\.([^\n]+)",
            m =>
            {
                var volume = m.Groups[1].Value;
                var parts = m.Groups[2].Value.Split("...");

                var results = new List<string>();

                foreach (var p in parts)
                {
                    var qtyMatch = Regex.Match(p, @"\d+");
                    if (!qtyMatch.Success) continue;

                    var qty = qtyMatch.Value;

                    var kind = Regex.IsMatch(p, @"(kd|it dg|it duong)")
                        ? "kd"
                        : "duong";

                    results.Add($"{qty} chai {volume}ml {kind}");
                }

                return string.Join(" ", results);
            });
    }
    private string Normalize(string input)
    {
        string text = input.ToLower();

        text = RemoveDiacritics(text);

        // fix spacing
        text = Regex.Replace(text, @"(\d)(\p{L})", "$1 $2");
        text = Regex.Replace(text, @"(\p{L})(\d)", "$1 $2");

        // normalize common variants
        text = text.Replace("kđ", "kd");
        text = text.Replace("it duong", "kd");
        text = text.Replace("k duong", "kd");
        text = text.Replace("it dg", "kd");

        // unify separators
        text = text.Replace("\n", " ");
        text = text.Replace("\r", " ");

        text = Regex.Replace(text, @"\s+", " ");

        return text.Trim();
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
