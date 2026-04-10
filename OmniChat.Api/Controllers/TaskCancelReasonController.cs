using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.TaskCancelReason;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class TaskCancelReasonController : BaseController<TaskCancelReasonController>
    {
        private readonly ITaskCancelReasonService _taskCancelReasonService;
        public TaskCancelReasonController(ILogger<TaskCancelReasonController> logger, ITaskCancelReasonService taskCancelReasonService) : base(logger)
        {
            _taskCancelReasonService = taskCancelReasonService;
        }

        [HttpGet(ApiEndPointConstant.TaskCancelReasonEndPoint.GetAllPaging)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<TaskCancelReasonResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy danh sách lý do hủy Task",
            Description = "Lấy danh sách lý do hủy Task với phân trang")]
        public async Task<IActionResult> GetAllTaskCancelReasonAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _taskCancelReasonService.GetAllTaskCancelReasonAsync(page, pageSize);
            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get Cancel Reason successfully", result);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpGet(ApiEndPointConstant.TaskCancelReasonEndPoint.GetBySupportTaskId)]
        [ProducesResponseType(typeof(ApiResponse<TaskCancelReasonResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy lý do hủy Task theo SupportTaskId",
            Description = "Lấy lý do hủy Task theo SupportTaskId")]
        public async Task<IActionResult> GetBySupportTaskIdAsync([FromRoute] Guid supportTaskId)
        {
            var result = await _taskCancelReasonService.GetTaskCancelReasonBySupportTaskIdAsync(supportTaskId);
            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get Cancel Reason successfully", result);
            return StatusCode(StatusCodes.Status200OK, response);
        }
    }
}
