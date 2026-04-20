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
            throw new BusinessException("Fail to parse messgae");

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
            catch (BusinessException e)
            {
                _logger.LogError("Failed to resolve batches for item: {Item}. Error: {Error}", item, e.Message);
                continue;
            }
        }
        if (orderItems.Count == 0 || orderItems == null)
            throw new BusinessException("Fail to auto draft");
        _logger.LogInformation("Creating draft order for customer {CustomerId} with {orderItems} items", customerId, orderItems);
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Name = $"Auto Draft for customer {customerId}",
            OrderItems = orderItems
        };  

        return await _orderService.CreateOrderAsync(request);
    }

    public async Task<CreateOrderRequest> TestCreateDraftOrderAsync(Guid customerId, string message)
    {
        var parsedItems = Parse(message);

        if (parsedItems == null || parsedItems.Count == 0)
            throw new BusinessException("Fail to parse messgae");

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
            catch (BusinessException e)
            {
                _logger.LogError("Failed to resolve batches for item: {Item}. Error: {Error}", item, e.Message);
                continue;
            }
        }
        if (orderItems.Count == 0 || orderItems == null)
            throw new BusinessException("Fail to auto draft");
        _logger.LogInformation("Creating draft order for customer {CustomerId} with {orderItems} items", customerId, orderItems);
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Name = $"Auto Draft for customer {customerId}",
            OrderItems = orderItems
        };
        return request;
    }
    private static Regex quantityRegex = new Regex(@"(\d+)\s*(chai|c|ch)\b", RegexOptions.Compiled);
    private static Regex volumeRegex = new Regex(@"(180|190|490|880|1760)\s*(ml)?|nhi|nua lit|1 lit|2 lit", RegexOptions.Compiled);
    private static Regex kindRegex = new Regex(@"(khong duong|ko duong|kd|co duong|duong|sua chua)", RegexOptions.Compiled);
    private static Regex brandRegex = new Regex(@"(long thanh|lothamilk|lt milk|lotha milk)", RegexOptions.Compiled);
    public List<DraftOrderItem> Parse(string raw)
    {
        var text = Normalize(raw);
        text = ExpandAllFormats(text);

        var results = new List<DraftOrderItem>();

        // 1. Find all valid volumes to use as anchors
        var volumeMatches = Regex.Matches(text, @"\b(180|190|490|880|1760)\b");

        foreach (Match volMatch in volumeMatches)
        {
            string volStr = volMatch.Value;

            // 2. Define a narrow window: 20 chars before and 25 chars after this specific volume
            int windowStart = Math.Max(0, volMatch.Index - 20);
            int windowEnd = Math.Min(text.Length, volMatch.Index + volMatch.Length + 25);
            string contextWindow = text.Substring(windowStart, windowEnd - windowStart);

            // 3. Extract Quantity from this specific window
            string qtyStr = "1";
            // Check for "[number] chai" or "so luong [number]" or just a number near the volume
            var qtyMatch = Regex.Match(contextWindow, @"(?<num>\d+)\s*(?:chai|c|ch)\b|so luong(?: la)?\s*(?<num>\d+)|(?<num2>\d+)\s+" + volStr);

            if (qtyMatch.Success)
            {
                qtyStr = qtyMatch.Groups["num"].Success ? qtyMatch.Groups["num"].Value : qtyMatch.Groups["num2"].Value;
            }
            else
            {
                // Fallback: the closest number in the window that isn't the volume itself
                var fallback = Regex.Matches(contextWindow, @"\b\d+\b")
                    .Cast<Match>()
                    .FirstOrDefault(n => n.Value != volStr);
                if (fallback != null) qtyStr = fallback.Value;
            }

            // 4. Create the item using the local context for Brand and Kind
            var item = new DraftOrderItem
            {
                Quantity = int.Parse(qtyStr),
                Volume = NormalizeVolume(volStr),
                Brand = DetectBrand(contextWindow) ?? "long thanh",
                Kind = DetectKind(contextWindow)
            };

            results.Add(item);
        }

        return results;
    }
    private List<DraftOrderItem> ParseSegment(string segment)
    {
        var results = new List<DraftOrderItem>();

        // Regex to find Quantity and Volume in any order
        // Matches: "2 chai 880", "880ml 2", "880 25c"
        var matches = Regex.Matches(segment,
            @"(?<qty>\d+)\s*(?:chai|c|ch)?\s*(?<vol>180|190|490|880|1760)\b|(?<vol2>180|190|490|880|1760)\s*(?:ml)?\s*(?<qty2>\d+)",
            RegexOptions.IgnoreCase);

        foreach (Match m in matches)
        {
            string volStr = m.Groups["vol"].Success ? m.Groups["vol"].Value : m.Groups["vol2"].Value;
            string qtyStr = m.Groups["qty"].Success ? m.Groups["qty"].Value : m.Groups["qty2"].Value;

            var item = new DraftOrderItem
            {
                Quantity = int.Parse(qtyStr),
                Volume = NormalizeVolume(volStr),
                Brand = DetectBrand(segment) ?? "long thanh",
                Kind = DetectKind(segment)
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
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        string text = input.ToLower();
        text = RemoveDiacritics(text);

        // Standardize all variants of "no sugar" and "sugar" immediately
        text = Regex.Replace(text, @"\b(k duong|ko duong|kd|k đ|kđ|it dg|it duong|it dduong|khong duong)\b", "kd");
        text = Regex.Replace(text, @"\b(co duong|dg|duong)\b", "duong");

        // Standardize product types
        text = Regex.Replace(text, @"\b(sua chua|sc)\b", "sua chua");
        text = Regex.Replace(text, @"\b(sua tuoi|st)\b", "st");

        // Ensure space between numbers and identifiers for easier regex matching
        text = Regex.Replace(text, @"(\d+)(ml|c|chai|ch|kd|duong|st|sc)", "$1 $2");
        text = Regex.Replace(text, @"(ml|c|chai|ch|kd|duong|st|sc)(\d+)", "$1 $2");

        return Regex.Replace(text, @"\s+", " ").Trim();
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
