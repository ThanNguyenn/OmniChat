using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage;
using OmniChat.Infrastructure.Dtos.Responses.SupportStaffMessage;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class SupportStaffMessageController : BaseController<SupportStaffMessageController>
    {
        private readonly ISupportStaffMessageService _supportStaffMessageService;

        public SupportStaffMessageController(ILogger<SupportStaffMessageController> logger, ISupportStaffMessageService supportStaffMessageService) : base(logger)
        {
            _supportStaffMessageService = supportStaffMessageService;
        }


        /// Get all support staff messages with pagination
        [HttpGet(ApiEndPointConstant.SupportStaffMessageEndPoint.GetAllPagingByStaffId)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllSupportStaffMessageResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy hết tin nhắn của staff gửi đến khách hàng có Paging",
            Description = "lấy tin nhắn của staff đã gửi đến khách hàng có Paging và research bằng staff Id"
            )]
        public async Task<IActionResult> GetAllSupportStaffMessagesPaging([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 20,[FromQuery] Guid? staffId = null)
        {
            var result = await _supportStaffMessageService.GetAllSupportStaffMessageByStaffIdAsync(
                pageNumber,
                pageSize,
                staffId);

            var response = ApiResponseBuilder.BuildResponse(
                StatusCodes.Status200OK, 
                "Lấy danh sách tin nhắn thành công", 
                result);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        /// Send message to Zalo
        [HttpPost(ApiEndPointConstant.SupportStaffMessageEndPoint.SendZaloMessage)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Gửi tin nhắn Zalo",
            Description = "Gửi tin nhắn của staff đến Zalo"
            )]
        public async Task<IActionResult> SendZaloMessage(
            [FromBody] CreateSupportStaffMessageRequest request)
        {
            if (!ModelState.IsValid)
                throw new BadRequestException("Dữ liệu tin nhắn không hợp lệ.");

            await _supportStaffMessageService.SendZaloMessageAsync(request);

            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK,
                "Gửi tin nhắn Zalo thành công",
                true);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        /// Send message to Facebook
        [HttpPost(ApiEndPointConstant.SupportStaffMessageEndPoint.SendFacebookMessage)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation(
            Summary = "Gửi tin nhắn Facebook",
            Description = "Gửi tin nhắn của staff đến Facebook"
            )]
        public async Task<IActionResult> SendFacebookMessage(
            [FromBody] CreateSupportStaffMessageRequest request)
        {
            await _supportStaffMessageService.SendFacebookMesageAsync(request);
            var response = ApiResponseBuilder.BuildResponse(
                StatusCodes.Status200OK, 
                "Gửi tin nhắn Facebook thành công", 
                true);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        // send message to Instagram
        [HttpPost(ApiEndPointConstant.SupportStaffMessageEndPoint.SendInstagramMessage)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation(
           Summary = "Gửi tin nhắn Instagram",
           Description = "Gửi tin nhắn của staff đến Instagram"
           )]
        public async Task<IActionResult> SendInstagramMessage(
           [FromBody] CreateSupportStaffMessageRequest request)
        {
            await _supportStaffMessageService.SendInstagramMesageAsync(request);
            return Ok(new { message = "Message sent to Instagram successfully" });
        }
    }
}
