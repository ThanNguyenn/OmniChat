using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Responses.Provider;
using OmniChat.Infrastructure.Metadatas;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
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
    }
}
