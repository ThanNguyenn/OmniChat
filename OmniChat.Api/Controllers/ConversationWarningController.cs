using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.WarningConversation;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class ConversationWarningController : BaseController<ConversationWarningController>
    {
        private readonly IConversationWarningService _conversationWarningService;

        public ConversationWarningController(ILogger<ConversationWarningController> logger, IConversationWarningService conversationWarningService) : base(logger)
        {
            _conversationWarningService = conversationWarningService;
        }

        [HttpGet(ApiEndPointConstant.ConversationWarning.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<WarningDetailRepsone>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy danh sách cảnh báo hội thoại (có phân trang)",
            Description = "Lấy danh sách các cảnh báo, có thể lọc theo trạng thái đã xem (isReviewed)"
        )]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isReviewed = null)
        {
           
            var warnings = await _conversationWarningService.GetAllWarningsAsync(pageNumber, pageSize, isReviewed);

          
            return Ok(new ApiResponse<PagingResponse<WarningDetailRepsone>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy danh sách cảnh báo thành công",
                IsSuccess = true,
                Data = warnings
            });
        }

        [HttpGet(ApiEndPointConstant.ConversationWarning.GetById)]
        [ProducesResponseType(typeof(ApiResponse<WarningDetailRepsone>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Lấy cảnh báo hội thoại",
            Description = "Lấy  cảnh báo, có thể lọc theo Id"
        )]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
        {
            var warning = await _conversationWarningService.GetWarningByIdAsync(id);

            return Ok(new ApiResponse<WarningDetailRepsone>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy thông tin chi tiết cảnh báo thành công",
                IsSuccess = true,
                Data = warning
            });
        }
    }
}
