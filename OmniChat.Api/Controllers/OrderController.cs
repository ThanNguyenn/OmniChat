using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Order;
using OmniChat.Infrastructure.Dtos.Responses.Order;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers;

[ApiController]
[Route(ApiEndPointConstant.Order.Base)]

public class OrderController : BaseController<OrderController>
{
    private readonly IOrderService _orderService;
    private readonly IDraftOrderService _draftOrderService;

    public OrderController(ILogger<OrderController> logger, IOrderService orderService, IDraftOrderService draftOrderService) : base(logger)
    {
        _orderService = orderService;
        _draftOrderService = draftOrderService;
    }

    [HttpPost(ApiEndPointConstant.Order.Create)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
    Summary = "Tạo mới order",
    Description = "Tạo mới order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest createOrderRequest)
    {
        var result = await _orderService.CreateOrderAsync(createOrderRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status201Created, "Order created successfully", result);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost(ApiEndPointConstant.Order.AutoDraft)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
    Summary = "Tạo mới draft order",
    Description = "Tạo mới draft order")]
    public async Task<IActionResult> CreateDraftOrder([FromBody] DraftOrderRequest createOrderRequest)
    {
        var result = await _draftOrderService.CreateDraftOrderAsync(createOrderRequest.CustomerId, createOrderRequest.Message);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status201Created, "Order created successfully", result);
        return StatusCode(StatusCodes.Status201Created, response);
    }
    //[HttpPost(ApiEndPointConstant.Order.Update)]
    //[ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    //[SwaggerOperation(
    //    Summary = "Cập nhật order",
    //    Description = "Cập nhật thông tin cơ bản của order theo id. Chỉ những field khác null trong request mới được cập nhật."
    //)]
    //public async Task<IActionResult> UpdateOrder([FromRoute] Guid id, [FromBody] UpdateOrderRequest updateOrderRequest)
    //{
    //    var result = await _orderService.UpdateOrderAsync(id, updateOrderRequest);
    //    var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Order updated successfully", result);
    //    return StatusCode(StatusCodes.Status200OK, response);
    //}

    [HttpPatch(ApiEndPointConstant.Order.CancelOrder)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Hủy order",
        Description = "Hủy order theo id đang delivery status pending và trả lại sản phẩm vào kho."
    )]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid id)
    {
        var result = await _orderService.CancelOrderAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Order cancelled successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPatch(ApiEndPointConstant.Order.CompleteDeliveredOrder)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteDeliveredOrder([FromRoute] Guid id)
    {
        var result = await _orderService.CompleteDeliverdOrderAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Order delivery status updated successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpDelete(ApiEndPointConstant.Order.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Xóa order",
        Description = "Xóa order theo id. Sau khi xóa, order sẽ không còn hiển thị trong danh sách."
    )]
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
    [SwaggerOperation(
        Summary = "Lấy order theo id",
        Description = "Lấy thông tin chi tiết của order dựa trên id."
    )]
    public async Task<IActionResult> GetOrderById([FromRoute] Guid id)
    {
        var result = await _orderService.GetOrderByIdAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Order retrieved successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Order.GetByIdForPostSale)]
    [ProducesResponseType(typeof(ApiResponse<GetOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Lấy order theo id cho ui refund",
        Description = "Lấy thông tin chi tiết của order cho ui refund dựa trên id."
    )]
    public async Task<IActionResult> GetOrderByIdForPostSale([FromRoute] Guid id)
    {
        var result = await _orderService.GetPostSaleOrderByIdAsync(id);
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
    [SwaggerOperation(
        Summary = "Lấy order theo customer id",
        Description = "Lấy thông tin chi tiết của order dựa trên customer id."
    )]
    public async Task<IActionResult> GetOrdersByCustomerId([FromRoute] Guid customerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string sortBy = "orderdate", [FromQuery] bool descending = true)
    {
        var result = await _orderService.GetOrdersByCustomerIdAsync(customerId, null, pageNumber, pageSize, sortBy, descending);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Orders retrieved successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Order.Dashboard)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DashboardOrderYearResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Lấy order dashboard info",
        Description = "Lấy order dashboard info theo datetime UTC. Input yyyy để lấy 12 tháng hoặc mm/yyyy để lấy theo tháng  "
    )]
    public async Task<IActionResult> GetOrderDashboardInfo([FromQuery] string input)
    {
        var result = await _orderService.GetDashboardAsync(input);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Order dashboard info retrieved successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);

    }

    [HttpGet(ApiEndPointConstant.Order.Shipper)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetOrderForShipperResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
      Summary = "Lấy order cho shipper",
      Description = "Lấy order cho shipper theo status Pending hoặc Completed."
    )]
    public async Task<IActionResult> GetOrdersForShipper([FromQuery] string? search, [FromQuery] int? pageNumber, [FromQuery] int? pageSize, [FromQuery] string? sortBy, [FromQuery] bool? descending)
    {
        var result = await _orderService.GetOrderForShipperAsync(search, pageNumber ?? 1, pageSize ?? 20, sortBy ?? "orderdate" , descending ?? true);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Orders for shipper retrieved successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }
}
