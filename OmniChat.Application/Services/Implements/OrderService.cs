using AutoMapper;
using ClosedXML;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Utils;
using OmniChat.Infrastructure.Dtos.Requests.Order;
using OmniChat.Infrastructure.Dtos.Requests.OrderItem;
using OmniChat.Infrastructure.Dtos.Responses.Order;
using OmniChat.Infrastructure.Dtos.Responses.OrderItem;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Dtos.Responses.Staff;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Implements;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class OrderService : BaseService<OrderService>, IOrderService
{
    private readonly ICreditNoteService creditNoteService;
    public OrderService(IUnitOfWork<OmniChatDbContext> unitOfWork,
        ILogger<OrderService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ICreditNoteService creditNoteService)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        this.creditNoteService = creditNoteService;
    }

    public async Task<bool> CreateOrderAsync(CreateOrderRequest request)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var productBatchRepo = _unitOfWork.GetRepository<ProductBatch>();
        var staffRepo = _unitOfWork.GetRepository<Staff>();

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = _mapper.Map<Order>(request);

            var batchIds = request.OrderItems
                .Select(x => x.ProductBatchId)
                .Distinct()
                .ToList();

            var batches = await productBatchRepo
                .GetQueryable(include: q => q.Include(b => b.Product))
                .Where(b => batchIds.Contains(b.Id))
                .ToListAsync();

            foreach (var item in request.OrderItems)
            {
                var batch = batches.FirstOrDefault(b => b.Id == item.ProductBatchId);

                if (batch == null)
                    throw new NotFoundException("Không tìm thấy lô sản phẩm");

                if (batch.Quantity < item.Quantity)
                    throw new BusinessException("Không đủ hàng");

                batch.Quantity -= item.Quantity;

                batch.Product.Quantity -= item.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    ProductBatchId = batch.Id,
                    Quantity = item.Quantity,
                    Price = batch.Product.Price
                });
            }
            //log the creator of the order

                var staff = await staffRepo.SingleOrDefaultAsync(predicate: s => s.AccountId == _httpContextAccessor.HttpContext.User.GetUserId());
            order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.Price);
            order.CreatorId = staff.Id;
            await orderRepo.InsertAsync(order);
        });

        return true;
    }

    public async Task<bool> DeleteOrderAsync(Guid orderId)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var order = await orderRepo.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new NotFoundException("Không tìm thấy đơn hàng");
                }
                order.IsDeleted = true;
                orderRepo.Update(order);
            });
        return true;
    }

    public Task<PagingResponse<GetAllOrdersResponse>> GetAllOrdersAsync(
        IEnumerable<OrderStatus>? orderStatuses,
        string? search,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "id",
        bool descending = false)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();

        var response = orderRepo.GetPagingListAsync<GetAllOrdersResponse>(
            predicate: o =>
                (orderStatuses == null || !orderStatuses.Any() || orderStatuses.Contains(o.Status)) &&
                (string.IsNullOrEmpty(search) ||
                 o.Code.Contains(search) ||
                 o.CustomerProfile.CustomerName!.Contains(search)),

            orderBy: q => OrderBy(q, sortBy, descending),

            selector: e => _mapper.Map<GetAllOrdersResponse>(e),

            include: q => q.Include(o => o.CustomerProfile),

            page: pageNumber,
            size: pageSize
        );

        return response;
    }

    public Task<PagingResponse<GetOrderResponse>> GetOrdersByCustomerIdAsync(
        Guid customerId,
        IEnumerable<OrderStatus>? orderStatuses,
        string? search,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "id",
        bool descending = false)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();

        var response = orderRepo.GetPagingListAsync<GetOrderResponse>(
            predicate: o =>
                o.CustomerId == customerId &&
                (orderStatuses == null || !orderStatuses.Any() || orderStatuses.Contains(o.Status)) &&
                (string.IsNullOrEmpty(search) || o.Code.Contains(search)),

            orderBy: q => OrderBy(q, sortBy, descending),

            include: q => q
                .Include(o => o.CustomerProfile)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductBatch)
                        .ThenInclude(pb => pb.Product),

            selector: e => _mapper.Map<GetOrderResponse>(e),

            page: pageNumber,
            size: pageSize
        );

        return response;
    }

    private static IOrderedQueryable<Order> OrderBy(IQueryable<Order> query, string sortBy, bool descending)
    {
        sortBy = sortBy?.Trim().ToLower() ?? "orderdate";

        Expression<Func<Order, object>> keySelector = sortBy switch
        {
            "name" => s => s.Name,
            "code" => s => s.Code,
            "totalamount" => s => s.TotalAmount,
            "status" => s => s.Status,
            "deliverystatus" => s => s.DeliveryStatus,
            "orderdate" => s => s.OrderDate,
            _ => s => s.OrderDate
        };

        return descending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }
    private async Task<TResponse> GetOrderByIdAsync<TResponse>(Guid orderId)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();

        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = await orderRepo.GetQueryable()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductBatch)
                        .ThenInclude(pb => pb.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new NotFoundException("Không tìm thấy đơn hàng");
            }

            return _mapper.Map<TResponse>(order);
        });
    }
    public async Task<GetOrderResponse> GetOrderByIdAsync(Guid orderId)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var response = await orderRepo.GetQueryable(predicate: o => o.Id == orderId, include: q => q.Include( q => q.CustomerProfile).Include(q => q.OrderItems).ThenInclude(q => q.ProductBatch).ThenInclude(q => q.Product)).FirstOrDefaultAsync();
        return _mapper.Map<GetOrderResponse>(response); 
    }

    public async Task<GetPostSaleOrderResponse> GetPostSaleOrderByIdAsync(Guid orderId)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var response = await orderRepo.GetQueryable(predicate: o => o.Id == orderId, include: q => q.Include(q => q.OrderItems).ThenInclude(q => q.ProductBatch).ThenInclude(q => q.Product)).FirstOrDefaultAsync();
        return _mapper.Map<GetPostSaleOrderResponse>(response);
    }
        

    public async Task<bool> UpdateOrderAsync(Guid orderId, UpdateOrderRequest updateOrderRequest)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var order = await orderRepo.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new NotFoundException("Không tìm thấy đơn hàng");
                }
                _mapper.Map(updateOrderRequest, order);
                orderRepo.Update(order);
            });
        return true;
    }

    public async Task<bool> CancelOrderAsync(Guid orderId)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var batchRepo = _unitOfWork.GetRepository<ProductBatch>();
        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = await orderRepo.SingleOrDefaultAsync(
                predicate: o => o.Id == orderId,
                include: q => q.Include(o => o.OrderItems));
            if (order == null)
                throw new NotFoundException("Không tìm thấy đơn hàng");
            if (order.Status != OrderStatus.Pending)
                throw new BusinessException("Chỉ các đơn hàng đang chờ xử lý mới có thể bị hủy.");
            await HandleBatchRestockAsync(order.OrderItems, batchRepo);
            order.Status = OrderStatus.Cancelled;
            orderRepo.Update(order);
            return true;
        });
    }

    private async Task HandleBatchRestockAsync(
        IEnumerable<OrderItem> orderItems,
        IGenericRepository<ProductBatch> batchRepo)
    {
        var batchIds = orderItems.Select(i => i.ProductBatchId).Distinct().ToList();

        var batches = (await batchRepo.GetListAsync(
            predicate: b => batchIds.Contains(b.Id),
            include: q => q.Include(b => b.Product) 
        )).ToList();

        if (batches.Count != batchIds.Count)
            throw new NotFoundException("Không tìm thấy lô sản phẩm");

        var batchDict = batches.ToDictionary(b => b.Id);

        foreach (var item in orderItems)
        {
            if (!batchDict.TryGetValue(item.ProductBatchId, out var batch))
                throw new NotFoundException("Không tìm thấy lô sản phẩm");

            batch.Quantity += item.Quantity;

            if (batch.Product == null)
                throw new NotFoundException("Không tìm thấy sản phẩm");

            batch.Product.Quantity += item.Quantity;
        }

        batchRepo.UpdateRange(batches);
    }

    public Task<bool> CompleteDeliverdOrderAsync(Guid orderId)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        return _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = await orderRepo.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("Không tìm thấy đơn hàng");
            }
            order.DeliveryStatus = DeliveryStatus.Completed;
            order.Status = OrderStatus.Shipped;
            order.DeliveriedDate = DateTime.UtcNow;
            orderRepo.Update(order);
            return true;
        });
    }

    public Task<bool> ReturnOrderPaidAsync(Guid orderId, double amount)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        return _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = await orderRepo.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("Không tìm thấy đơn hàng");
            }
            order.Status = OrderStatus.Returned;
            
            orderRepo.Update(order);
            await _unitOfWork.CommitAsync();
            _unitOfWork.Context.ChangeTracker.Clear();
            await creditNoteService.CreateCreditNoteRefundAsync(orderId, amount);
            return true;
        });
    }

    public Task<bool> ReturnOrderUnpaidAsync(Guid orderId, double amount)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        return _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = await orderRepo.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("Không tìm thấy đơn hàng");
            }
            order.Status = OrderStatus.Returned;
            orderRepo.Update(order);
            await creditNoteService.CreateCreditNoteAdjustmentAsync(orderId, amount);
            return true;
        });
    }

    public async Task<IEnumerable<DashboardOrderYearResponse>> GetDashboardAsync(IEnumerable<string>? status, string input)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();

        var results = new List<DashboardOrderYearResponse>();

        bool isYear = input.Length == 4;

        int year;
        int? month = null;

        if (isYear)
        {
            year = int.Parse(input);
        }
        else
        {
            var parts = input.Split('/');
            month = int.Parse(parts[0]);
            year = int.Parse(parts[1]);
        }

        var monthsToProcess = isYear
            ? Enumerable.Range(1, 12)
            : new[] { month.Value };

        HashSet<OrderStatus>? statusFilter = null;

        if (status != null && status.Any())
        {
            statusFilter = status
                .Select(s => s.Trim().ToLower())
                .Select(s => s switch
                {
                    "completed" => OrderStatus.Completed,
                    "cancelled" => OrderStatus.Cancelled,
                    "returned" => OrderStatus.Returned,
                    _ => (OrderStatus?)null
                })
                .Where(s => s.HasValue)
                .Select(s => s.Value)
                .ToHashSet();
        }

        foreach (var m in monthsToProcess)
        {
            var fromDate = DateTime.SpecifyKind(new DateTime(year, m, 1), DateTimeKind.Utc);
            var toDateExclusive = fromDate.AddMonths(1);

            var query = orderRepo.GetQueryable(
        o => o.OrderDate >= fromDate &&
             o.OrderDate < toDateExclusive,
            asNoTracking: true
            )
            .Select(o => new
            {
                MappedStatus =
                    o.Status == OrderStatus.Completed ? OrderStatus.Completed :
                    o.Status == OrderStatus.Cancelled ? OrderStatus.Cancelled :
                    (o.Status == OrderStatus.PendingReturn ||
                     o.Status == OrderStatus.Returned ||
                     o.Status == OrderStatus.ReturnedDefective)
                        ? OrderStatus.Returned
                        : (OrderStatus?)null
            })
            .Where(x => x.MappedStatus != null);

            if (statusFilter != null)
            {
                query = query.Where(x => statusFilter.Contains(x.MappedStatus.Value));
            }

            var data = await query
                .GroupBy(x => x.MappedStatus.Value)
                .Select(g => new GetOrderDashBoardByStatus
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            results.Add(new DashboardOrderYearResponse
            {
                Month = $"{m:D2}/{year}",
                Status = data
            });
        }

        return results;
    }

    public Task<PagingResponse<GetOrderForShipperResponse>> GetOrderForShipperAsync(string? status, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false)
    {

        var orderRepo = _unitOfWork.GetRepository<Order>();
        var response = orderRepo.GetPagingListAsync<GetOrderForShipperResponse>(
            predicate: o => string.IsNullOrEmpty(status) || o.DeliveryStatus.ToString()!.Contains(status),
            orderBy: q => OrderBy(q, sortBy, descending),
            selector: e => _mapper.Map<GetOrderForShipperResponse>(e),
             include: q => q.Include(o => o.CustomerProfile).Include(o => o.OrderItems)
                                                                    .ThenInclude(oi => oi.ProductBatch)
                                                                            .ThenInclude(pb => pb.Product),
            page: pageNumber,
            size: pageSize);

        return response;
    }

    public async Task<PagingResponse<GetOrderForShipperResponse>> GetPendingOrderShipperIdAsync(Guid shipperId, int pageNumber = 1, int pageSize = 20)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();

        var response = await orderRepo.GetPagingListAsync<GetOrderForShipperResponse>(
            predicate: o => o.DriverId == shipperId && o.DeliveryStatus == DeliveryStatus.Pending,
            orderBy: q => q.OrderByDescending(o => o.OrderDate),
            selector: e => _mapper.Map<GetOrderForShipperResponse>(e),
            include: q => q.Include(o => o.CustomerProfile).Include(o => o.OrderItems)
                                                                    .ThenInclude(oi => oi.ProductBatch)
                                                                            .ThenInclude(pb => pb.Product),
            page: pageNumber,
            size: pageSize);

        return response;
    }


    public async Task<PagingResponse<GetOrderForShipperResponse>> OrderShipperHistory(Guid shipperId, int pageNumber = 1, int pageSize = 20)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();

        var response = await orderRepo.GetPagingListAsync<GetOrderForShipperResponse>(
            predicate: o => o.DriverId == shipperId && o.DeliveryStatus == DeliveryStatus.Completed,
            orderBy: q => q.OrderByDescending(o => o.OrderDate),
            selector: e => _mapper.Map<GetOrderForShipperResponse>(e),
             include: q => q.Include(o => o.CustomerProfile).Include(o => o.OrderItems)
                                                                     .ThenInclude(oi => oi.ProductBatch)
                                                                            .ThenInclude(pb => pb.Product),
            page: pageNumber,
            size: pageSize);

        return response;
    }

    public async Task<ShipperDeliveredReportResponse> GetDeliveredReportAsync(Guid shipperId,DateTime? fromDate,DateTime? toDate,int pageNumber = 1,int pageSize = 20)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();

       
        var start = fromDate?.Date ?? DateTime.MinValue;
        var end = toDate?.Date ?? DateTime.MaxValue;

        
        Expression<Func<Order, bool>> filter = o => o.DriverId == shipperId &&
                                                    o.DeliveryStatus == DeliveryStatus.Completed &&
                                                    o.DeliveriedDate != null &&
                                                    o.DeliveriedDate.Value.Date >= start &&
                                                    o.DeliveriedDate.Value.Date <= end;

        
        var totalCount = await orderRepo.CountAsync(filter);

       
        var pagedOrders = await orderRepo.GetPagingListAsync<GetOrderResponse>(
            predicate: filter,
            orderBy: q => q.OrderByDescending(o => o.DeliveriedDate),
            selector: e => _mapper.Map<GetOrderResponse>(e),
            include: q => q.Include(o => o.CustomerProfile)
                           .Include(o => o.OrderItems)
                                .ThenInclude(oi => oi.ProductBatch)
                                    .ThenInclude(pb => pb.Product),
            page: pageNumber,
            size: pageSize);

       
        return new ShipperDeliveredReportResponse
        {
            TotalDeliveredOrders = totalCount,
            Orders = pagedOrders
        };
    }


    public async Task<bool> SubmitOrderAsync(Guid orderId)
    {
        var  orderRepo = _unitOfWork.GetRepository<Order>();
        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = await orderRepo.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("Không tìm thấy đơn hàng");
            }
            if (order.Status != OrderStatus.Draft)
            {
                throw new BusinessException("Chỉ các đơn hàng đang chờ xử lý mới có thể nộp.;
            }
            order.Status = OrderStatus.Pending;
            orderRepo.Update(order);
            return true;
        });
    }

    public async Task<bool> AddOrderItemAsync(Guid orderId, AddOrderItemRequest request)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var batchRepo = _unitOfWork.GetRepository<ProductBatch>();

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = await orderRepo
                .GetQueryable(include: o => o.Include(x => x.OrderItems))
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new NotFoundException("Không tìm thấy đơn hàng");

            var batch = await batchRepo
                .GetQueryable(include: b => b.Include(x => x.Product))
                .FirstOrDefaultAsync(b => b.Id == request.ProductBatchId);

            if (batch == null)
                throw new NotFoundException("Không tìm thấy lô sản phẩm");

            if (batch.Quantity < request.Quantity)
                throw new BusinessException("Không đủ hàng");

            var existingItem = order.OrderItems
                .FirstOrDefault(x => x.ProductBatchId == request.ProductBatchId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductBatchId = batch.Id,
                    Quantity = request.Quantity,
                    Price = batch.Product.Price
                });
            }

            batch.Quantity -= request.Quantity;
            batch.Product.Quantity -= request.Quantity;

            order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.Price);
        });

        return true;
    }

    public async Task<bool> RemoveOrderItemAsync(Guid orderId, Guid orderItemId)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var batchRepo = _unitOfWork.GetRepository<ProductBatch>();

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = await orderRepo
                .GetQueryable(include: o => o.Include(x => x.OrderItems))
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new NotFoundException("Không tìm thấy đơn hàng");

            var orderItem = order.OrderItems
                .FirstOrDefault(i => i.Id == orderItemId);


            var batch = await batchRepo
                .GetQueryable(include: b => b.Include(x => x.Product))
                .FirstOrDefaultAsync(b => b.Id == orderItem.ProductBatchId);

            if (batch == null)
                throw new NotFoundException("Không tìm thấy lô sản phẩm");

            // restore stock
            batch.Quantity += orderItem.Quantity;
            batch.Product.Quantity += orderItem.Quantity;

            order.OrderItems.Remove(orderItem);

            order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.Price);
        });

        return true;
    }

    public async Task<bool> UpdateOrderItemAsync(
        Guid orderId,
        Guid orderItemId,
        UpdateOrderItemRequest request)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var batchRepo = _unitOfWork.GetRepository<ProductBatch>();

        if (request.Quantity == 0)
            return await RemoveOrderItemAsync(orderId, orderItemId);

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = await orderRepo
                .GetQueryable(include: o => o.Include(x => x.OrderItems))
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new NotFoundException("Không tìm thấy đơn hàng");

            var orderItem = order.OrderItems
                .FirstOrDefault(i => i.Id == orderItemId);

            if (orderItem == null)
                throw new NotFoundException("Không tìm thấy sảm phẩm trong đơn hàng");

            var batch = await batchRepo
                .GetQueryable(include: b => b.Include(x => x.Product))
                .FirstOrDefaultAsync(b => b.Id == orderItem.ProductBatchId);

            if (batch == null)
                throw new NotFoundException("Không tìm thấy lô sản phẩm");

            int delta = request.Quantity - orderItem.Quantity;

            if (delta > 0)
            {
                if (batch.Quantity < delta)
                    throw new BusinessException("Không đủ hàng");

                batch.Quantity -= delta;
                batch.Product.Quantity -= delta;
            }
            else if (delta < 0)
            {
                batch.Quantity += Math.Abs(delta);
                batch.Product.Quantity += Math.Abs(delta);
            }

            orderItem.Quantity = request.Quantity;

            order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.Price);
        });

        return true;
    }
}
