using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.FeedBack;
using OmniChat.Infrastructure.Dtos.Responses.FeedBack;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class FeedbackController : BaseController<FeedbackController>
    {
        private readonly IFeedBackService _feedBackService;
        public FeedbackController(ILogger<FeedbackController> logger, IFeedBackService feedBackService) : base(logger)
        {
            _feedBackService = feedBackService;
        }


        [HttpGet(ApiEndPointConstant.FeedBackEndPoint.GetByStaffId)] 
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<FeedBackResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Lấy danh sách FeedBack theo Staff",
        Description = "Lấy danh sách FeedBack được khách hàng gửi về theo từng nhân viên hỗ trợ")]
        public async Task<IActionResult> GetFeedBackByStaffIdAsync(
        [FromRoute] Guid id,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
        {
            var result = await _feedBackService.GetFeedBackByStaffIdAsync(id, pageIndex, pageSize);
            return Ok(new ApiResponse<PagingResponse<FeedBackResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy danh sách FeedBack thành công",
                IsSuccess = true,
                Data = result
            });
        }
        [HttpGet(ApiEndPointConstant.FeedBackEndPoint.GetById)]
        [ProducesResponseType(typeof(ApiResponse<FeedBackResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
              Summary = "Lấy thông tin chi tiết FeedBack",
              Description = "Lấy thông tin chi tiết FeedBack được khách hàng gửi về")]
        public async Task<IActionResult> GetFeedBackByIdAsync([FromRoute] Guid id)
        {
            var result = await _feedBackService.GetFeedBackByIdAsync(id);
            if (result is null)
                return NotFound(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Không tìm thấy FeedBack với Id '{id}'",
                    IsSuccess = false,
                    Data = null
                });

            return Ok(new ApiResponse<FeedBackResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy thông tin chi tiết FeedBack thành công",
                IsSuccess = true,
                Data = result
            });
        }


        [HttpPost(ApiEndPointConstant.FeedBackEndPoint.Create)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
         Summary = "Nhận feedback từ form của khách hàng",
         Description = "Khách hàng gửi phản hồi sau cuộc hội thoại. StaffId và FormUrl được tự động lấy từ hệ thống.")]
        public async Task<IActionResult> CreateFeedBack(
         [FromRoute] Guid conversationId,
         [FromBody] FeedBackRequest request)
        {
            var formUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var success = await _feedBackService.ErichFeedBackFormAsync(conversationId, request, formUrl);

            return StatusCode(StatusCodes.Status201Created, new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Gửi phản hồi thành công",
                IsSuccess = success,
                Data = success
            });
        }

        [HttpGet(ApiEndPointConstant.FeedBackEndPoint.GetLink)]
        public IActionResult GenerateFeedbackLink([FromRoute] Guid conversationId)
        {
         
            var vercelUrl = "https://your-feedback-form.vercel.app";

            var feedbackLink = $"{vercelUrl}?conversationId={conversationId}";

            return Ok(new ApiResponse<string>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Khởi tạo link feedback thành công",
                IsSuccess = true,
                Data = feedbackLink
            });
        }
    }
}