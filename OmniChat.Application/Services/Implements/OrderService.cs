using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Utils;
using OmniChat.Infrastructure.Dtos.Requests.Order;
using OmniChat.Infrastructure.Dtos.Responses.Order;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
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
                    throw new NotFoundException("Product batch not found");

                if (batch.Quantity < item.Quantity)
                    throw new BusinessException("Insufficient stock");

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
                    throw new NotFoundException("Order not found");
                }
                order.IsDeleted = true;
                orderRepo.Update(order);
            });
        return true;
    }

    public Task<PagingResponse<GetAllOrdersResponse>> GetAllOrdersAsync(string? search, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var response = orderRepo.GetPagingListAsync<GetAllOrdersResponse>(
            predicate: o => string.IsNullOrEmpty(search) || o.Code.Contains(search),
            orderBy: q => OrderBy(q, sortBy, descending),
            selector: e => _mapper.Map<GetAllOrdersResponse>(e),
            page: pageNumber,
            size: pageSize);

        return response;
    }

    public Task<PagingResponse<GetOrderResponse>> GetOrdersByCustomerIdAsync(Guid customerId, string? search, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var response = orderRepo.GetPagingListAsync<GetOrderResponse>(
            predicate: o => o.CustomerId == customerId &&
                            (string.IsNullOrEmpty(search) || o.Code.Contains(search)),
            orderBy: q => OrderBy(q, sortBy, descending),
            include: q => q
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductBatch)
                        .ThenInclude(pb => pb.Product),
            selector: e => _mapper.Map<GetOrderResponse>(e),
            page: pageNumber,
            size: pageSize);
        return response;
    }

    private static IOrderedQueryable<Order> OrderBy(IQueryable<Order> query, string sortBy, bool descending)
    {
        sortBy = sortBy?.Trim().ToLower() ?? "id";

        Expression<Func<Order, object>> keySelector = sortBy switch
        {
            "name" => s => s.Name,
            "code" => s => s.Code,
            "totalamount" => s => s.TotalAmount,
            "status" => s => s.Status,
            "deliverystatus" => s => s.DeliveryStatus,
            "orderdate" => s => s.OrderDate,
            _ => s => s.Id
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
                throw new NotFoundException("Order not found");
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

    public Task<GetPostSaleOrderResponse> GetPostSaleOrderByIdAsync(Guid orderId)
    {
        return GetOrderByIdAsync<GetPostSaleOrderResponse>(orderId);
    }

    public async Task<bool> UpdateOrderAsync(Guid orderId, UpdateOrderRequest updateOrderRequest)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var order = await orderRepo.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new NotFoundException("Order not found");
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
                throw new NotFoundException("Order not found");
            if (order.Status == OrderStatus.Cancelled)
                throw new BusinessException("Order already cancelled");
            if (order.Status != OrderStatus.Pending)
                throw new BusinessException("Only pending orders can be cancelled");
            await HandleBatchRestockAsync(order.OrderItems, batchRepo);
            order.Status = OrderStatus.Cancelled;
            orderRepo.Update(order);
            return true;
        });
    }

    private async Task HandleBatchRestockAsync(IEnumerable<OrderItem> orderItems, IGenericRepository<ProductBatch> batchRepo)
    {
        var batchIds = orderItems.Select(i => i.ProductBatchId).Distinct().ToList();
        var batches = (await batchRepo.GetListAsync(predicate: b => batchIds.Contains(b.Id))).ToList();
        if (batches.Count != batchIds.Count)
            throw new NotFoundException("One or more product batches associated with this order are missing.");
        var batchDict = batches.ToDictionary(b => b.Id);
        foreach (var item in orderItems)
        {
            if (!batchDict.TryGetValue(item.ProductBatchId, out var batch))
                throw new NotFoundException("Product batch not found");

            batch.Quantity += item.Quantity;
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
                throw new NotFoundException("Order not found");
            }
            order.DeliveryStatus = DeliveryStatus.Completed;
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
                throw new NotFoundException("Order not found");
            }
            order.Status = OrderStatus.Returned;
            
            orderRepo.Update(order);
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
                throw new NotFoundException("Order not found");
            }
            order.Status = OrderStatus.Returned;
            orderRepo.Update(order);
            await creditNoteService.CreateCreditNoteAdjustmentAsync(orderId, amount);
            return true;
        });
    }

    public async Task<IEnumerable<GetOrderDashBoardByStatus>> GetOrderDashBoardByStatusesAsync(DateTime from, DateTime to)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();

        var fromDate = from.Date;
        var toDateExclusive = to.Date.AddDays(1);

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
            .Where(x => x.MappedStatus != null)
            .GroupBy(x => x.MappedStatus.Value)
            .Select(g => new GetOrderDashBoardByStatus
            {
                Status = g.Key,
                Count = g.Count()
            });

        return await query.ToListAsync();
    }

    public Task<PagingResponse<GetOrderForShipperResponse>> GetOrderForShipperAsync(string? status, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false)
    {

        var orderRepo = _unitOfWork.GetRepository<Order>();
        var response = orderRepo.GetPagingListAsync<GetOrderForShipperResponse>(
            predicate: o => string.IsNullOrEmpty(status) || o.DeliveryStatus.ToString()!.Contains(status),
            orderBy: q => OrderBy(q, sortBy, descending),
            selector: e => _mapper.Map<GetOrderForShipperResponse>(e),
            page: pageNumber,
            size: pageSize);

        return response;
    }
}
