using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Claim;
using OmniChat.Infrastructure.Dtos.Responses.TaskAction;
using OmniChat.Infrastructure.Metadatas;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class TaskActionController : Controller
    {
        private readonly ITaskActionService _taskActionService;

        public TaskActionController(ITaskActionService taskActionService)
        {
            _taskActionService = taskActionService;
        }

        [HttpGet(ApiEndPointConstant.TaskActionEndPoint.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<TaskActionResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _taskActionService.GetAllTaskActionAsync(pageIndex, pageSize);
            return Ok(new ApiResponse<PagingResponse<TaskActionResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy danh sách hành động thành công",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpGet(ApiEndPointConstant.TaskActionEndPoint.GetById)]
        [ProducesResponseType(typeof(ApiResponse<TaskActionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var result = await _taskActionService.GetTaskActionByIdAsync(id);
            return Ok(new ApiResponse<TaskActionResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy thông tin hành động thành công",
                IsSuccess = true,
                Data = result
            });
        }
    }
}
