using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Responses.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Responses.Provider;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class CustomerProfileController : BaseController<CustomerProfileController>
    {
        private readonly ICustomerProfileService _customerProfileService;
        public CustomerProfileController(ILogger<CustomerProfileController> logger, ICustomerProfileService customerProfileService) : base(logger)
        {
            _customerProfileService = customerProfileService;
        }

        [HttpGet(ApiEndPointConstant.CustomerProfileEndPoint.GetAllCustomerProfileByCustomerName)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetCustomerProfileResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy Profile của Customer có Paging",
            Description = "Lấy Profile của Customer có Paging và research bằng customer Name"
            )]
        public async Task<IActionResult>GetAllCustomerProfileByCustomerNamePagingAsync(
      [FromQuery] string? customerName = null,
      [FromQuery] int pageNumber = 1,
      [FromQuery] int pageSize = 20)
        {
            var result =
                await _customerProfileService
                    .GetCustomerProfilesPagingAsync(
                        pageNumber,
                        pageSize,
                        customerName
                    );

            return Ok(new ApiResponse<PagingResponse<GetCustomerProfileResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get customer profiles successfully",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpPost(ApiEndPointConstant.CustomerProfileEndPoint.MergeAndDeleteCustomerProfile)]
        [ProducesResponseType(
                typeof(ApiResponse<GetCustomerProfileResponse>),
                StatusCodes.Status200OK)]
                    [SwaggerOperation(
                Summary = "Gộp và xóa Customer Profile",
                Description =
                    "Gộp profile mới tạo (Facebook/Instagram) vào profile đã tồn tại, " +
                    "update message & conversation, sau đó xóa profile nguồn"
            )]
        public async Task<IActionResult> MergeAndDeleteCustomerProfileAsync(
    [FromBody] MergeCustomerProfileRequest request)
        {
            var result =
                await _customerProfileService.MergeAndDeleteAsync(
                    request.SourceCustomerId,
                    request.TargetCustomerId
                );

            return Ok(new ApiResponse<GetCustomerProfileResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Merge customer profile successfully",
                IsSuccess = true,
                Data = result
            });
        }
    }
}
