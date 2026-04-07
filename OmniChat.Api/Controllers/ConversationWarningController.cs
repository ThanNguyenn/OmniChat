using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.WarningConversation;
using OmniChat.Infrastructure.Metadatas;

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
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<WarningDetailRepsone>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] bool? isReviewed = null)
        {
            var warnings = await _conversationWarningService.GetAllWarningsAsync(isReviewed);
          
            return Ok(new ApiResponse<IEnumerable<WarningDetailRepsone>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get All Conversation Warning Successfully",
                IsSuccess = true,
                Data = warnings
            });
        }

        [HttpGet(ApiEndPointConstant.ConversationWarning.GetById)]
        [ProducesResponseType(typeof(ApiResponse<WarningDetailRepsone>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
        {
            var warning = await _conversationWarningService.GetWarningByIdAsync(id);

            return Ok(new ApiResponse<WarningDetailRepsone>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get Conversation Warning By Id Successfully",
                IsSuccess = true,
                Data = warning
            });
        }
    }
}
