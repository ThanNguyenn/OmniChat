using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
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
        Description = "Hoàn thành Support Task bằng Task Id")]
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
    }
}
