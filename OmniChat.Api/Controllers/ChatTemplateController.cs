using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.ChatTemplate;
using OmniChat.Infrastructure.Dtos.Responses.ChatTemplate;
using OmniChat.Infrastructure.Metadatas;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class ChatTemplateController : BaseController<ChatTemplateController>
    {
        private readonly IChatTemplateService _chatTemplateService;

        public ChatTemplateController(ILogger<ChatTemplateController> logger, IChatTemplateService chatTemplateService) : base(logger)
        {
            _chatTemplateService = chatTemplateService;
        }

        [HttpGet(ApiEndPointConstant.ChatTemplate.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<ChatTemplateResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllChatTemplateAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var result = await _chatTemplateService.GetAllChatTemplateAsync(pageNumber, pageSize, search);
            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem ChatTemplate thành công", result);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpGet(ApiEndPointConstant.ChatTemplate.GetById)]
        [ProducesResponseType(typeof(ApiResponse<ChatTemplateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChatTemplateByIdAsync([FromRoute] Guid id)
        {
            var result = await _chatTemplateService.GetChatTemplateByIdAsync(id);
            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem ChatTemplate thành công", result);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpPost(ApiEndPointConstant.ChatTemplate.Create)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateChatTemplateAsync([FromBody] ChatTemplateRequest request)
        {
            var result = await _chatTemplateService.CreateChatTemplateAsync(request);

            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status201Created, "Tạo ChatTemplate thành công", result);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut(ApiEndPointConstant.ChatTemplate.Update)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChatTemplateAsync([FromRoute] Guid id, [FromBody] ChatTemplateRequest request)
        {
            var result = await _chatTemplateService.UpdateChatTemplateAsync(id, request);
            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Cập nhật ChatTemplate thành công", result);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpDelete(ApiEndPointConstant.ChatTemplate.Delete)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteChatTemplateAsync([FromRoute] Guid id)
        {
            var result = await _chatTemplateService.DeleteChatTemplateAsync(id);
            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xóa ChatTemplate thành công", result);
            return StatusCode(StatusCodes.Status200OK, response);
        }
    }
}
