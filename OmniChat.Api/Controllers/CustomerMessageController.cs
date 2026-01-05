using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.CustomerMessage;
using OmniChat.Infrastructure.Metadatas;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class CustomerMessageController : BaseController<CustomerMessageController>
    {
        private readonly ICustomerMessageService _customerMessageService;

        public CustomerMessageController(ILogger<CustomerMessageController> logger, ICustomerMessageService customerMessageService) : base(logger)
        {
            _customerMessageService = customerMessageService;
        }

        [HttpGet(ApiEndPointConstant.CustomerMessageEndPoint.GetAllPagingByCustomerId)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult>GetAllCustomerMessageByCustomerIdPagingAsync(
            [FromQuery] Guid customerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result =
                await _customerMessageService
                    .GetAllCustomerMessageByCustomerIdAsync(
                        pageNumber,
                        pageSize,
                        customerId
                    );

            if (customerId == Guid.Empty)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = 400,
                    Message = "customerId is required",
                    IsSuccess = false
                });
            }

            return Ok(new ApiResponse<PagingResponse<GetAllCustomerMessageResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get customer messages successfully",
                IsSuccess = true,
                Data = result
            });
        }

    }
}
