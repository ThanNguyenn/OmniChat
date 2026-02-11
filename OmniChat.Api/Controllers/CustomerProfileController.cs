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

        [HttpGet(ApiEndPointConstant.CustomerProfileEndPoint.GetCustomerByEmailOrPhone)]
        [ProducesResponseType(typeof(ApiResponse<GetCustomerProfileResponse>),StatusCodes.Status200OK)]
        [SwaggerOperation(
           Summary = "Tìm Customer theo Email hoặc Phone",
           Description = "Dùng để tìm customer đã tồn tại trước khi thực hiện merge"
         )]
        public async Task<IActionResult> GetCustomerByEmailOrPhoneAsync(
       [FromQuery] string keyword)
        {
            var result =
                await _customerProfileService
                    .GetCustomerProfileByEmailOrPhoneAsync(keyword);

            return Ok(new ApiResponse<GetCustomerProfileResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Get customer profile successfully",
                Data = result
            });
        }

        [HttpPut(ApiEndPointConstant.CustomerProfileEndPoint.UpdateCustomerProfile)]
        [ProducesResponseType(
        typeof(ApiResponse<GetCustomerProfileResponse>),
        StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Cập nhật thông tin Customer",
            Description = "Cập nhật thông tin Customer theo CustomerId"
        )]
        public async Task<IActionResult> UpdateCustomerProfileAsync(
        [FromRoute] Guid customerId,
        [FromBody] UpdateCustomerProfileRequest request)
        {
            var result = await _customerProfileService
                .UpdateCustomerProfileByIdAsync(customerId, request);

            return Ok(new ApiResponse<GetCustomerProfileResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Customer profile updated successfully",
                Data = result
            });
        }
    }
}
