using OmniChat.Infrastructure.Dtos.Requests.Order;
using OmniChat.Infrastructure.Dtos.Responses.Order;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IOrderService
{
    Task<bool> CreateOrderAsync(CreateOrderRequest createOrderRequest); 
    Task<bool> DeleteOrderAsync(Guid orderId);
    Task<bool> UpdateOrderAsync(Guid orderId, UpdateOrderRequest updateOrderRequest);
    Task<PagingResponse<GetOrderResponse>> GetOrdersByCustomerIdAsync(Guid customerId, string? search, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false);
    Task<GetOrderResponse> GetOrderByIdAsync(Guid orderId);
    Task<GetPostSaleOrderResponse> GetPostSaleOrderByIdAsync(Guid orderId);
    Task<PagingResponse<GetAllOrdersResponse>> GetAllOrdersAsync(string? search, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false);
    Task<bool> CancelOrderAsync(Guid orderId);
    Task<bool> CompleteDeliverdOrderAsync(Guid orderId);
    Task<bool> ReturnOrderPaidAsync(Guid orderId, double amount);
    Task<bool> ReturnOrderUnpaidAsync(Guid orderId, double amount);

    Task<IEnumerable<DashboardOrderYearResponse>> GetDashboardAsync(IEnumerable<string>? status,string input);

    Task<PagingResponse<GetOrderForShipperResponse>> GetOrderForShipperAsync(string? status, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false);
}
