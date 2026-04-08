using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Staff;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    [Route(ApiEndPointConstant.Shipper.Base)]
    public class ShipperController : BaseController<ShipperController>
    {
        private readonly IStaffService _staffService;
        public ShipperController(ILogger<ShipperController> logger, IStaffService staffService) : base(logger)
        {
            _staffService = staffService;
        }

        [HttpGet(ApiEndPointConstant.Shipper.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<ShipperResposne>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
      Summary = "Lấy list shipper",
      Description = "Lấy list shipper theo filter: pageIndex, pageSize")]

        public async Task<IActionResult> GetShippers([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _staffService.GetShippersAsync(pageIndex, pageSize);

            return Ok(new ApiResponse<PagingResponse<ShipperResposne>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Retrieved shippers successfully",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpPost(ApiEndPointConstant.Shipper.AssignOrder)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
           Summary = "Assign order to shipper",
           Description = "Assign order to shipper by shipperId and orderId")]
        public async Task<IActionResult> AssignOrderToShipper([FromQuery] Guid id, [FromQuery] Guid orderId)
        {
            await _staffService.AssignShipperOrderAsync(id, orderId);

            return Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Assigned order to shipper successfully",
                IsSuccess = true,
                Data = null
            });
        }

        [HttpGet(ApiEndPointConstant.Shipper.GetById)]
        [ProducesResponseType(typeof(ApiResponse<ShipperResposne>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
           Summary = "Lấy thông tin shipper theo id",
           Description = "Lấy thông tin shipper theo id")]
        public async Task<IActionResult> GetShipperByIdAsync([FromRoute] Guid id)
        {
            var result = await _staffService.GetShipperByShipperIdAsync(id);

            return Ok(new ApiResponse<ShipperResposne>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Retrieved shipper successfully",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpGet(ApiEndPointConstant.Shipper.Dashboard)]
        [ProducesResponseType(typeof(ApiResponse<ShipperDashboardResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
           Summary = "Lấy dashboard của shipper",
           Description = "Lấy dashboard của shipper : ActiveShippers, DeliveringOrders, DeliveredToday ")]
        public async Task<IActionResult> GetShipperDashboard()
        {
            var result = await _staffService.GetShipperDashboardAsync();

            return Ok(new ApiResponse<ShipperDashboardResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Retrieved shipper dashboard successfully",
                IsSuccess = true,
                Data = result
            });
        }

    }
}
