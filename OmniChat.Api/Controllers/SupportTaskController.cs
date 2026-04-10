using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.TaskCancelReason;
using OmniChat.Infrastructure.Dtos.Responses.SupportTask;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class SupportTaskController : BaseController<SupportTaskController>
    {
        private readonly ISupportTaskService _supportTaskService;
        public SupportTaskController(ILogger<SupportTaskController> logger, ISupportTaskService supportTaskService) : base(logger)
        {
            _supportTaskService = supportTaskService;
        }

        [HttpPatch(ApiEndPointConstant.SupportTaskEndPoint.CompleteSupportTask)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
        Summary = "Complete Support Task",
        Description = "Complete Support Task bằng Task Id")]
        public async Task<IActionResult> CompleteTask(Guid id)
        {
            var result = await _supportTaskService.CompleteTaskAsync(id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Complete SupportTask Successfully",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpPatch(ApiEndPointConstant.SupportTaskEndPoint.CancelSupportTask)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]

        [SwaggerOperation(
            Summary = "Cancel Support Task",
            Description = "Cancel Support Task bằng Task Id và lý do hủy"
            )]
        public async Task<IActionResult> CancelSupportTaskAsync(
        [FromRoute] Guid id,
        [FromBody] TaskCancelReasonRequest cancelReasonRequest)
        {
            var result = await _supportTaskService.CancelSupportTaskAsync(id, cancelReasonRequest);
            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Cancel Support Task successfully", result);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpGet(ApiEndPointConstant.SupportTaskEndPoint.GetSupportTaskByConversationId)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SupportTasksResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
        Summary = "Get Support Task Inprocess, Reassign, Done On Conversation",
        Description = "Lấy tất cả các Support Task của Conversation bằng Conversation Id")]
        public async Task<IActionResult> GetSupportTaskByConversationId(Guid conversationId)
        {
            var result = await _supportTaskService.GetSupportTaskOnConversationIdAsync(conversationId);
            return Ok(new ApiResponse<IEnumerable<SupportTasksResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get Support Task Successfully",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpGet(ApiEndPointConstant.SupportTaskEndPoint.GetTaskIntentDashboard)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaskIntentDashboardResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Get Support Task intent và count",
            Description = "Get Support Task intent và count theo datetime UTC")]
        public async Task<IActionResult> GetTaskIntentDashboard([FromQuery]DateTime from, DateTime to)
        {
            var result = await _supportTaskService.GetTaskIntentDashboardResponsesAsync(from, to);
            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get Task Intent Dashboard Successfully", result);
            return Ok(response);
        }
    }
}