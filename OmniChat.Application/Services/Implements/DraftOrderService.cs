using AutoMapper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OmniChat.Application.Services.Implements;

public class DraftOrderService : BaseService<DraftOrderService>, IDraftOrderService
{
    private readonly IOrderService _orderService;
    public DraftOrderService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<DraftOrderService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IOrderService orderService) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _orderService = orderService;
    }

    public async Task<bool> CreateDraftOrderFromConversationAsync(Guid conversationId)
    {
        var conversationRepo = _unitOfWork.GetRepository<SupportConversation>();

        var conversation = await conversationRepo
            .GetQueryable(
                predicate: c => c.Id == conversationId,
                include: q => q.Include(c => c.CustomerMessages))
            .FirstOrDefaultAsync() ?? throw new NotFoundException("Không tìm thấy cuộc hội thoại"); ;       

        var customerId = conversation.ActiveCustomerId;

        var messages = conversation.CustomerMessages
            .OrderBy(m => m.Timestamp)
            .Select(m => m.Content)
            .ToList();

        if (!HasOrderConfirmation(messages))
            throw new BusinessException("Khách hàng chưa xác nhận đơn hàng, vui lòng xác nhận với khách hàng trước khi tạo đơn");

        return await CreateDraftOrderFromConversationAsync(customerId, messages);
    }

    public async Task<bool> CreateDraftOrderFromConversationNewAsync(Guid customerId, IEnumerable<string> messages)
    {
        if (!HasOrderConfirmation(messages.ToList()))
            throw new BusinessException("Khách hàng chưa xác nhận đơn hàng, vui lòng xác nhận với khách hàng trước khi tạo đơn");

        return await CreateDraftOrderFromConversationAsync(customerId, messages.ToList());
    }

    public async Task<List<DraftOrderItem>> PreviewDraftOrderAsync(Guid customerId, List<string> messages)
    {
        var context = new DraftOrderContext { CustomerId = customerId };

        if (!HasOrderConfirmation(messages))
            throw new BusinessException("Khách hàng chưa xác nhận đơn hàng, vui lòng xác nhận với khách hàng trước khi tạo đơn");

        foreach (var msg in messages)
        {
            var parsed = Parse(msg);
            ApplyToContext(context, parsed, msg);
        }

        return await Task.FromResult(context.Items);
    }

    private async Task<bool> CreateDraftOrderFromConversationAsync(
        Guid customerId,
        List<string> messages)
    {
        var context = new DraftOrderContext { CustomerId = customerId };

        foreach (var msg in messages)
        {
            var parsed = Parse(msg);
            ApplyToContext(context, parsed, msg);
        }

        return await CreateOrderFromContext(context);
    }

    private bool HasOrderConfirmation(List<string> messages)
    {
        if (messages == null || messages.Count == 0)
            return false;

        var lastFive = messages
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .TakeLast(5)
            .Select(x => Normalize(x))
            .ToList();

        string[] keywords =
        {
            "ok",
            "duoc",
            "chap nhan",
            "nhat tri",
            "cu vay di",
            "nhu the di",
            "chot",
            "dong y",
            "oke",
            "okee",
            "okkk"
        };

        foreach (var msg in lastFive)
        {
            foreach (var keyword in keywords)
            {
                if (Regex.IsMatch(msg, $@"\b{Regex.Escape(keyword)}\b"))
                    return true;
            }
        }

        return false;
    }


    private void ApplyToContext(DraftOrderContext context, List<DraftOrderItem> newItems, string message)
    {
        newItems ??= new List<DraftOrderItem>();
        var normalizedMsg = Normalize(message);

        if (IsCancellation(normalizedMsg))
        {
            RemoveItems(context, newItems);
            context.LastFocusedItem = null;
            return;
        }

        if (newItems.Count == 0 && context.LastFocusedItem != null)
        {
            var standaloneQty = ExtractStandaloneQuantity(normalizedMsg);

            if (standaloneQty.HasValue)
            {
                if (IsSubtraction(normalizedMsg))
                {
                    context.LastFocusedItem.Quantity = Math.Max(0, context.LastFocusedItem.Quantity - standaloneQty.Value);
                    if (context.LastFocusedItem.Quantity == 0) context.Items.Remove(context.LastFocusedItem);
                    return;
                }

                if (IsAddIntent(normalizedMsg))
                {
                    context.LastFocusedItem.Quantity += standaloneQty.Value;
                    return;
                }

                context.LastFocusedItem.Quantity = standaloneQty.Value;
                return;
            }

            if (normalizedMsg.Contains("kd") || normalizedMsg.Contains("duong") || normalizedMsg.Contains("sua chua"))
            {
                context.LastFocusedItem.Kind = DetectKind(normalizedMsg);
                return;
            }
        }

        if (newItems.Count > 0)
        {
            if (IsAddIntent(normalizedMsg))
            {
                AddOrMerge(context, newItems);
            }
            else
            {
                OverwriteItems(context, newItems);
            }
            context.LastFocusedItem = context.Items.LastOrDefault();
        }
    }

    private int? ExtractStandaloneQuantity(string message)
    {
        var normalized = Normalize(message);

        var match = Regex.Match(normalized, @"\b(?:them|lay|lay cho minh|so luong)?\s*(?<num>\d+)\b(?:\s*(?:chai|c|ch))?");

        if (match.Success && int.TryParse(match.Groups["num"].Value, out int qty))
        {
            if (qty == 180 || qty == 490 || qty == 880 || qty == 1760) return null;

            return qty;
        }
        return null;
    }

    private void AddOrMerge(DraftOrderContext context, List<DraftOrderItem> items)
    {
        foreach (var item in items)
        {
            var existing = context.Items.FirstOrDefault(x =>
                x.Volume == item.Volume &&
                x.Brand == item.Brand &&
                x.Kind == item.Kind);

            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                context.Items.Add(item);
        }
    }

    private void OverwriteItems(DraftOrderContext context, List<DraftOrderItem> items)
    {
        foreach (var item in items)
        {
            context.Items.RemoveAll(x =>
                x.Volume == item.Volume &&
                x.Brand == item.Brand &&
                x.Kind == item.Kind);

            context.Items.Add(item);
        }
    }

    private void RemoveItems(DraftOrderContext context, List<DraftOrderItem> items)
    {
        if (items == null || items.Count == 0)
        {
            context.Items.Clear();
            return;
        }

        foreach (var item in items)
        {
            context.Items.RemoveAll(x =>
                x.Volume == item.Volume &&
                x.Brand == item.Brand &&
                x.Kind == item.Kind);
        }
    }


    private bool IsCancellation(string msg)
    {
        return Regex.IsMatch(msg, @"\b(khong lay|huy|xoa|bo het)\b");
    }
    private bool IsSubtraction(string msg) => Regex.IsMatch(msg, @"\b(bo ra|bot|tru|bot di)\b");
    private bool IsAddIntent(string msg)
    {
        return Regex.IsMatch(msg, @"\b(them|add|\+)\b");
    }

    private async Task<bool> CreateOrderFromContext(DraftOrderContext context)
    {
        var orderItems = new List<AddOrderItemRequest>();

        foreach (var item in context.Items)
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

        if (!orderItems.Any())
            throw new BusinessException("Tự động tạo đon thất bại");

        return await _orderService.CreateOrderAsync(new CreateOrderRequest
        {
            CustomerId = context.CustomerId,
            Name = $"Auto Draft for {context.CustomerId}",
            OrderItems = orderItems
        });
    }

    public async Task<bool> CreateDraftOrderAsync(Guid customerId, string message)
    {
        var parsedItems = Parse(message);

        if (parsedItems == null || parsedItems.Count == 0)
            throw new BusinessException("Tự động tạo đon thất bại");

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
            throw new BusinessException("Tự động tạo đon thất bại");
        _logger.LogInformation("Creating draft order for customer {CustomerId} with {orderItems} items", customerId, orderItems);
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Name = $"Auto Draft for customer {customerId}",
            OrderItems = orderItems
        };  

        return await _orderService.CreateOrderAsync(request);
    }

    private static Regex brandRegex = new Regex(@"(long thanh|lothamilk|lt milk|lotha milk|lotha)", RegexOptions.Compiled);
    public List<DraftOrderItem> Parse(string raw)
    {
        var text = Normalize(raw);
        text = ExpandAllFormats(text);

        var results = new List<DraftOrderItem>();

        var volumeMatches = Regex.Matches(text, @"\b(180|190|490|880|1760)\b");

        for (int i = 0; i < volumeMatches.Count; i++)
        {
            Match volMatch = volumeMatches[i];
            string volStr = volMatch.Value;

            int windowStart = Math.Max(0, volMatch.Index - 20);
            int windowEnd = Math.Min(text.Length, volMatch.Index + volMatch.Length + 25);

            if (i > 0)
            {
                int prevEnd = volumeMatches[i - 1].Index + volumeMatches[i - 1].Length;
                windowStart = Math.Max(windowStart, prevEnd);
            }

            if (i < volumeMatches.Count - 1)
            {
                int nextVolIndex = volumeMatches[i + 1].Index;
                string textBetween = text.Substring(volMatch.Index + volMatch.Length, nextVolIndex - (volMatch.Index + volMatch.Length));

                var nextQuantityMatch = Regex.Match(textBetween, @"\b\d+\s*(?:chai|c|ch)?\b");
                if (nextQuantityMatch.Success)
                {
                    int cutoffIndex = volMatch.Index + volMatch.Length + nextQuantityMatch.Index;
                    windowEnd = Math.Min(windowEnd, cutoffIndex);
                }
                else
                {
                    windowEnd = Math.Min(windowEnd, nextVolIndex);
                }
            }

            string contextWindow = text.Substring(windowStart, windowEnd - windowStart);

            string qtyStr = "1";

            var qtyMatch = Regex.Match(contextWindow, @"(?<num>\d+)\s*(?:chai|c|ch)\b|so luong(?: la)?\s*(?<num>\d+)|(?<num2>\d+)\s+" + volStr);

            if (qtyMatch.Success)
            {
                qtyStr = qtyMatch.Groups["num"].Success ? qtyMatch.Groups["num"].Value : qtyMatch.Groups["num2"].Value;
            }
            else
            {
                var fallback = Regex.Matches(contextWindow, @"\b\d+\b")
                    .Cast<Match>()
                    .FirstOrDefault(n => n.Value != volStr);
                if (fallback != null) qtyStr = fallback.Value;
            }

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

    private string DetectKind(string text)
    {
        if (Regex.IsMatch(text, @"(sua chua|^y\b|sc)"))
            return "yogurt";

        if (Regex.IsMatch(text, @"\bkd\b") || text.Contains("khong duong") || text.Contains("k duong") || text.Contains("ko duong"))
            return "no_sugar";

        if (text.Contains("duong") && !text.Contains("kd"))
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

        text = Regex.Replace(text, @"\b(khong duong|k duong|ko duong|kd|k đ|kđ|it dg|it duong|it dduong)\b", "kd");

        text = Regex.Replace(text, @"\b(co duong|dg|duong)\b", "duong");

        text = Regex.Replace(text, @"\b(sua chua|sc)\b", "sua chua");
        text = Regex.Replace(text, @"\b(sua tuoi|st)\b", "st");

        text = Regex.Replace(text, @"(\d+)(ml|c|chai|ch|kd|duong|st|sc)", "$1 $2");
        text = Regex.Replace(text, @"(ml|c|chai|ch|kd|duong|st|sc)(\d+)", "$1 $2");

        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    private string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var chars = normalized.Where(c => Char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        var result = new string(chars.ToArray()).Normalize(NormalizationForm.FormC);

        return result.Replace('đ', 'd').Replace('Đ', 'D');
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

  

    private string NormalizeBrand(string b)
    {
        b = b.Trim();

        if (b.Contains("lothamilk") || b.Contains("lotha") || b.Contains("lotha milk"))
            return "lothamilk";

        return "long thanh";
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
            .OrderBy(b => b.ManuFactureDate)
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
