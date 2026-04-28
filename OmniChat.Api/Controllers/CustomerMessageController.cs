using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.CustomerMessage;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

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
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllCustomerMessageResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy hết tin nhắn của Customer có Paging",
            Description = "Lấy tin nhắn của Customer có Paging và research bằng CustomerId"
            )]
        public async Task<IActionResult>GetAllCustomerMessageByCustomerIdPagingAsync([FromQuery] Guid customerId,[FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 20)
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
                    Message = "Mã khách hàng (CustomerId) là bắt buộc và không được để trống.",
                    IsSuccess = false
                });
            }

            return Ok(new ApiResponse<PagingResponse<GetAllCustomerMessageResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy danh sách tin nhắn thành công",
                IsSuccess = true,
                Data = result
            });
        }

    }
}
    