using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
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
    public OrderService(IUnitOfWork<OmniChatDbContext> unitOfWork,
        ILogger<OrderService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task<bool> CreateOrderAsync(CreateOrderRequest request)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var productBatchRepo = _unitOfWork.GetRepository<ProductBatch>();

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var lastOrder = await orderRepo
              .GetQueryable()
              .OrderByDescending(p => p.Code)
              .FirstOrDefaultAsync();
            int lastCode =
                lastOrder != null &&
                int.TryParse(lastOrder.Code.AsSpan(3), out var codeValue)
                    ? codeValue
                    : 0;
            var newCode = GenerateOrderCode(lastCode);

            var order = _mapper.Map<Order>(request);
            order.Code = newCode;

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
            await orderRepo.InsertAsync(order);
        });

        return true;
    }

    private string GenerateOrderCode(int lastCode)
    {
        return (lastCode + 1).ToString("D6");
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
            _ => s => s.Id
        };

        return descending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }

    public Task<GetOrderResponse> GetOrderByIdAsync(Guid orderId)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        return _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = await orderRepo.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("Order not found");
            }
            var response = _mapper.Map<GetOrderResponse>(order);
            return response;
        });
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

    public Task<bool> CancelOrderAsync(Guid orderId, OrderStatus newStatus)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        return _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = await orderRepo.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("Order not found");
            }
            order.Status = newStatus;
            orderRepo.Update(order);
            return true;
        });
    }

    public Task<bool> CompleteDeliverdOrderAsync(Guid orderId, DeliveryStatus newDeliveredStatus)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        return _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var order = await orderRepo.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("Order not found");
            }
            order.DeliveryStatus = newDeliveredStatus;
            orderRepo.Update(order);
            return true;
        });
    }
}
