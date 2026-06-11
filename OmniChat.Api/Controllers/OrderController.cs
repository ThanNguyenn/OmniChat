using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Order;
using OmniChat.Infrastructure.Dtos.Requests.OrderItem;
using OmniChat.Infrastructure.Dtos.Responses.Order;
using OmniChat.Infrastructure.Dtos.Responses.Staff;
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
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status201Created, "Tạo đơn hàng thành công", result);
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
        var result = await _draftOrderService.CreateDraftOrderFromConversationAsync(createOrderRequest.ConversationId);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status201Created, "Tạo đơn hàng thành công", result);
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
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Hủy đơn hàng thành công", result);
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
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Giao hàng thành công", result);
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
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xóa đơn hàng thành công", result);
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
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem đơn hàng thành công", result);
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
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem đơn hàng thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Order.GetAll)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllOrdersResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllOrders([FromQuery] IEnumerable<OrderStatus>? orderStatuses, [FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string sortBy = "orderdate", [FromQuery] bool descending = true)
    {
        var result = await _orderService.GetAllOrdersAsync(orderStatuses, search, pageNumber, pageSize, sortBy, descending);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem danh sách đơn hàng thành công", result);
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
    public async Task<IActionResult> GetOrdersByCustomerId([FromRoute] Guid customerId, [FromQuery] IEnumerable<OrderStatus>? orderStatuses, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string sortBy = "orderdate", [FromQuery] bool descending = true)
    {
        var result = await _orderService.GetOrdersByCustomerIdAsync(customerId, orderStatuses, null, pageNumber, pageSize, sortBy, descending);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem danh sách đơn hàng thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Order.Dashboard)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DashboardOrderYearResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Lấy order dashboard info",
        Description = "Lấy order dashboard info theo datetime UTC. Input yyyy để lấy 12 tháng hoặc mm/yyyy để lấy theo tháng. Status là returned, cancelled completed "
    )]
    public async Task<IActionResult> GetOrderDashboardInfo([FromQuery] IEnumerable<string>? status, [FromQuery] string input)
    {
        var result = await _orderService.GetDashboardAsync(status, input);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem thống kê đơn hàng thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);

    }

    [Authorize]
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
        var result = await _orderService.GetOrderForShipperAsync(search, pageNumber ?? 1, pageSize ?? 20, sortBy ?? "orderdate", descending ?? true);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem danh sách đơn hàng thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPost(ApiEndPointConstant.Order.SubmitDraft)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Submit draft order",
        Description = "Submit draft order id rồi chuyền  status từ draft thành pending."
    )]
    public async Task<IActionResult> SubmitDraftOrder([FromRoute] Guid id)
    {
        var result = await _orderService.SubmitOrderAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Nộp đơn hàng nháp thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPost(ApiEndPointConstant.Order.AddOrderItem)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Add Order Item",
        Description = "Add order item theo order id."
    )]
    public async Task<IActionResult> AddOrderItem([FromRoute] Guid id, [FromBody] AddOrderItemRequest addOrderItemRequest)
    {
        var result = await _orderService.AddOrderItemAsync(id, addOrderItemRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Tạo sản phẩm vào đơn hàng thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPut(ApiEndPointConstant.Order.UpdateOrderItem)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update Order Item",
        Description = "Update order item theo order id và order item id đã add."
    )]
    public async Task<IActionResult> UpdateOrderItem([FromRoute] Guid orderId, [FromRoute] Guid orderItemId, [FromBody] UpdateOrderItemRequest updateOrderItemRequest)
    {
        var result = await _orderService.UpdateOrderItemAsync(orderId, orderItemId, updateOrderItemRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Cập nhật sản phẩm trong đơn hàng thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpDelete(ApiEndPointConstant.Order.RemoveOrderItem)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Delete Order Item",
        Description = "Delete order item theo order id và order item id đã add."
    )]
    public async Task<IActionResult> RemoveOrderItem([FromRoute] Guid orderId, [FromRoute] Guid orderItemId)
    {
        var result = await _orderService.RemoveOrderItemAsync(orderId, orderItemId);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xóa sản phẩm trong đơn hàng thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }


    [HttpGet(ApiEndPointConstant.Order.GetPendingByShipper)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetOrderForShipperResponse>>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Get Pending Order of Shipper",
        Description = "Lấy danh sách đơn hàng đang chờ giao theo shipper id."
    )]
    public async Task<IActionResult> GetPendingOrderByShipper([FromRoute] Guid shipperId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _orderService.GetPendingOrderShipperIdAsync(shipperId, pageNumber, pageSize);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem danh sách đơn hàng thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Order.GetHistoryByShipper)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetOrderForShipperResponse>>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Get Order History of Shipper",
        Description = "Lấy danh sách lịch sử đơn hàng đã giao theo shipper id."
    )]
    public async Task<IActionResult> GetHistoryOrderByShipper([FromRoute] Guid shipperId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _orderService.OrderShipperHistory(shipperId, pageNumber, pageSize);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem danh sách đơn hàng thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Order.CountDelivered)]
    [ProducesResponseType(typeof(ApiResponse<ShipperDeliveredReportResponse>), StatusCodes.Status200OK)]
    [SwaggerOperation(
     Summary = "Get Shipper Delivered Report",
     Description = "Lấy tổng số lượng và danh sách chi tiết đơn hàng đã giao thành công theo khoảng thời gian."
    )]
    public async Task<IActionResult> GetShipperDeliveredReport(
     [FromQuery] Guid shipperId,
     [FromQuery] DateTime? fromDate,
     [FromQuery] DateTime? toDate,
     [FromQuery] int pageNumber = 1,
     [FromQuery] int pageSize = 20)
    {
        var result = await _orderService.GetDeliveredReportAsync(shipperId, fromDate, toDate, pageNumber, pageSize);

        var response = ApiResponseBuilder.BuildResponse(
            StatusCodes.Status200OK,
            "Xem thống kê giao hàng thành công",
            result
        );

        return Ok(response);
    }

    [HttpPost("preview")]
    public async Task<ActionResult<List<DraftOrderItem>>> PreviewDraftOrder(
         [FromBody] List<string> messages)
    {
        if (messages == null || !messages.Any())
        {
            return BadRequest("Message list cannot be empty.");
        }
        Guid customerId = Guid.NewGuid(); // Replace with actual customer ID retrieval logic

        var result = await _draftOrderService.PreviewDraftOrderAsync(customerId, messages);
        return Ok(result);
    }

    [HttpGet(ApiEndPointConstant.Order.GetOrdersInInvoice)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InvoiceOrderResponse>>), StatusCodes.Status200OK)]
    [SwaggerOperation(
     Summary = "Get Orders in Invoice",
     Description = "Lấy danh sách đơn hàng trong một hóa đơn cụ thể."
    )]
    public async Task<IActionResult> GetOrdersInInvoice([FromRoute] Guid invoiceId)
    {
        var result = await _orderService.GetOrdersINvoiceAsync(invoiceId);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem danh sách đơn hàng trong hóa đơn thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }
}
