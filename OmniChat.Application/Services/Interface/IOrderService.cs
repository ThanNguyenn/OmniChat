using OmniChat.Infrastructure.Dtos.Requests.Order;
using OmniChat.Infrastructure.Dtos.Requests.OrderItem;
using OmniChat.Infrastructure.Dtos.Responses.Order;
using OmniChat.Infrastructure.Dtos.Responses.Staff;
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
    Task<PagingResponse<GetOrderResponse>> GetOrdersByCustomerIdAsync(Guid customerId, IEnumerable<OrderStatus>? orderStatuses, string? search, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false);
    Task<PagingResponse<GetAllOrdersResponse>> GetAllOrdersAsync(IEnumerable<OrderStatus>? orderStatuses, string? search, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false);
    Task<GetOrderResponse> GetOrderByIdAsync(Guid orderId);
    Task<GetPostSaleOrderResponse> GetPostSaleOrderByIdAsync(Guid orderId);
    Task<bool> CancelOrderAsync(Guid orderId);
    Task<bool> CompleteDeliverdOrderAsync(Guid orderId);
    Task<bool> ReturnOrderPaidAsync(Guid orderId, double amount);
    Task<bool> ReturnOrderUnpaidAsync(Guid orderId, double amount);

    Task<bool> SubmitOrderAsync(Guid orderId);

    Task<IEnumerable<DashboardOrderYearResponse>> GetDashboardAsync(IEnumerable<string>? status, string input);

    Task<PagingResponse<GetOrderForShipperResponse>> GetOrderForShipperAsync(string? status, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false);

    Task<bool> AddOrderItemAsync(Guid orderId, AddOrderItemRequest addOrderItemRequest);

    Task<bool> UpdateOrderItemAsync(Guid orderId, Guid orderItemId, UpdateOrderItemRequest updateOrderItemRequest);

    Task<bool> RemoveOrderItemAsync(Guid orderId, Guid orderItemId);

    public Task<PagingResponse<GetOrderForShipperResponse>> GetPendingOrderShipperIdAsync(Guid shipperId, int pageNumber = 1, int pageSize = 20);

    public Task<PagingResponse<GetOrderForShipperResponse>> OrderShipperHistory(Guid shipperId, int pageNumber = 1, int pageSize = 20);

    public  Task<ShipperDeliveredReportResponse> GetDeliveredReportAsync(Guid shipperId, DateTime? fromDate, DateTime? toDate, int pageNumber = 1, int pageSize = 20);

    public  Task<IEnumerable<InvoiceOrderResponse>> GetOrdersINvoiceAsync(Guid invoiceId);

}
