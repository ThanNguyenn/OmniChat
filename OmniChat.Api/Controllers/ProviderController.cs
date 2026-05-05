using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Provider;
using OmniChat.Infrastructure.Dtos.Responses.Provider;
using OmniChat.Infrastructure.Dtos.Responses.SupportStaffMessage;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class ProviderController : BaseController<ProviderController>
    {
        private readonly IProviderService _providerService;
        public ProviderController(ILogger<ProviderController> logger, IProviderService providerService) : base(logger)
        {
            _providerService = providerService;
        }

        [HttpPost(ApiEndPointConstant.ProviderEndPoint.CreateProvider)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Tạo mới Provier",
            Description = "Tạo mới Provider "
            )]
        public async Task<IActionResult> CreateProviderAsync([FromBody] CreateProviderRequest createProviderRequest)
        {
            await _providerService.CreateProviderAsync(createProviderRequest);

            return StatusCode(StatusCodes.Status201Created, new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Tạo kênh chat liên kết thành công",
                IsSuccess = true,
                Data = true
            });
        }

        [HttpGet(ApiEndPointConstant.ProviderEndPoint.GetAllPagingByproviderName)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllProviderResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy tất cả Provider Paging",
            Description = "Lấy tất cả thông tin Provider có Paging và search theo ProviderName "
            )]
        public async Task<IActionResult> GetAllPagingByProviderNameAsync([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 20,[FromQuery] string? providerName = null)
        {
            var result =
                await _providerService.GetAllProviderAsync(
                    pageNumber,
                    pageSize,
                    providerName
                );

            return Ok(new ApiResponse<PagingResponse<GetAllProviderResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy danh sáchkênh chat liên kết thành công",
                IsSuccess = true,
                Data = result
            });
        }

    }
}
