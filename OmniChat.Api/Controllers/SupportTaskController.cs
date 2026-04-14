using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.TaskCancelReason;
using OmniChat.Infrastructure.Dtos.Responses.SupportTask;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
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
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConversationTaskResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
        Summary = "Get Support Task Inprocess, Reassign, Done On Conversation",
        Description = "Lấy tất cả các Support Task của Conversation bằng Conversation Id")]
        public async Task<IActionResult> GetSupportTaskByConversationId(Guid conversationId)
        {
            var result = await _supportTaskService.GetSupportTaskOnConversationIdAsync(conversationId);
            return Ok(new ApiResponse<IEnumerable<ConversationTaskResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get Support Task Successfully",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpGet(ApiEndPointConstant.SupportTaskEndPoint.GetTaskIntentDashboard)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DashboardMonthResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Get Support Task intent và count",
            Description = "Get Support Task intent và count theo datetime UTC. Input yyyy để lấy 12 tháng hoặc mm/yyyy để lấy theo tháng ")]
        public async Task<IActionResult> GetTaskIntentDashboard([FromQuery] string period)
        {
            var result = await _supportTaskService.GetTaskIntentDashboardResponsesAsync(period);
            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get Task Intent Dashboard Successfully", result);
            return Ok(response);
        }

        [HttpGet(ApiEndPointConstant.SupportTaskEndPoint.GetTaskStatus)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DashboardMonthResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Get Support Task follow status và count",
            Description = "Get Support Task theo status và count theo datetime UTC. Input yyyy để lấy 12 tháng hoặc mm/yyyy để lấy theo tháng ")]
        public async Task<IActionResult> GetTaskStatusDashboard([FromQuery] string year, [FromQuery] SupportTaskStatus status)
        {
            var result = await _supportTaskService.GetTaskTotalByStatusDashboardAsync(year, status);
            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get Task Intent Dashboard Successfully", result);
            return Ok(response);
        }
    }
}