using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class CustomerMergeController : Controller
    {
        private readonly ICustomerMergeService _customerMergeService;

        public CustomerMergeController(
            ICustomerMergeService customerMergeService)
        {
            _customerMergeService = customerMergeService;
        }

        [HttpPost(ApiEndPointConstant.CustomerServiceMergeEndpoint.CustomerMerge)]
        [ProducesResponseType(
            typeof(ApiResponse<GetCustomerProfileResponse>),
            StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Gộp và xóa Customer Profile",
            Description =
                "Gộp profile mới tạo (Facebook/Instagram) vào profile đã tồn tại, " +
                "update message & conversation, sau đó xóa profile nguồn"
        )]
        public async Task<IActionResult> MergeCustomerAsync(
            [FromBody] MergeCustomerProfileRequest request)
        {
            var result =
                await _customerMergeService.MergeAndDeleteAsync(
                    request.SourceCustomerId,
                    request.TargetCustomerId
                );

            return Ok(new ApiResponse<GetCustomerProfileResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Merge customer profile successfully",
                Data = result
            });
        }
    }
}
