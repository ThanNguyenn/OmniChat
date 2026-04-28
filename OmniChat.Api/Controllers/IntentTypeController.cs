using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Claim;
using OmniChat.Infrastructure.Dtos.Responses.IntentType;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class IntentTypeController : BaseController<IntentTypeController>
    {
        private readonly IIntentTypeService _intentTypeService;
       
        public IntentTypeController(ILogger<IntentTypeController> logger,IIntentTypeService intentTypeService) : base(logger)
        {
            _intentTypeService = intentTypeService;
        }

        [HttpGet(ApiEndPointConstant.IntentType.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetsIntentTypeResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy IntentTypes",
            Description = "Lấy danh sách IntentTypes")]

        public async Task<IActionResult> GetIntentTypesAsync()
        {
            var response = await _intentTypeService.GetIntentTypesAsync();

            return Ok(new ApiResponse<IEnumerable<GetsIntentTypeResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy danh sách loại yêu cầu thành công",
                IsSuccess = true,
                Data = response
            });
        }

    }
}
