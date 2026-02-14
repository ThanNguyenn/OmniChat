using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Order;
using OmniChat.Infrastructure.Dtos.Responses.Order;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;

namespace OmniChat.Api.Controllers;

[ApiController]
[Route(ApiEndPointConstant.Order.Base)]

public class OrderController : BaseController<OrderController>
{
    private readonly IOrderService _orderService;

    public OrderController(ILogger<OrderController> logger, IOrderService orderService) : base(logger)
    {
        _orderService = orderService;
    }

    [HttpPost(ApiEndPointConstant.Order.Create)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest createOrderRequest)
    {
        var result = await _orderService.CreateOrderAsync(createOrderRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status201Created, "Order created successfully", result);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost(ApiEndPointConstant.Order.Update)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrder([FromRoute] Guid id, [FromBody] UpdateOrderRequest updateOrderRequest)
    {
        var result = await _orderService.UpdateOrderAsync(id, updateOrderRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Order updated successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPatch(ApiEndPointConstant.Order.CancelOrder)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await _orderService.CancelOrderAsync(id, request.NewStatus);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Order cancelled successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPatch(ApiEndPointConstant.Order.CompleteDeliveredOrder)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteDeliveredOrder([FromRoute] Guid id, [FromBody] UpdateOrderDeliveryStatusRequest request)
    {
        var result = await _orderService.CompleteDeliverdOrderAsync(id, request.NewDeliveryStatus);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Order delivery status updated successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpDelete(ApiEndPointConstant.Order.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteOrder([FromRoute] Guid id)
    {
        var result = await _orderService.DeleteOrderAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Order deleted successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Order.GetById)]
    [ProducesResponseType(typeof(ApiResponse<GetOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderById([FromRoute] Guid id)
    {
        var result = await _orderService.GetOrderByIdAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Order retrieved successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Order.GetAll)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllOrdersResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllOrders([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string sortBy = "id", [FromQuery] bool descending = false)
    {
        var result = await _orderService.GetAllOrdersAsync(search, pageNumber, pageSize, sortBy, descending);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Orders retrieved successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Order.GetByCustomerId)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetOrderResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrdersByCustomerId([FromRoute] Guid customerId, [FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string sortBy = "id", [FromQuery] bool descending = false)
    {
        var result = await _orderService.GetOrdersByCustomerIdAsync(customerId, search, pageNumber, pageSize, sortBy, descending);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Orders retrieved successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }
}
