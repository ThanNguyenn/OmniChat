using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Wallet;
using OmniChat.Infrastructure.Dtos.Responses.Wallet;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers;


[ApiController]
[Route(ApiEndPointConstant.Wallet.Base)]
public class WalletController : BaseController<WalletController>
{
    private readonly IWalletService _walletService;
    public WalletController(ILogger<WalletController> logger, IWalletService walletService) : base(logger)
    {
        _walletService = walletService;
    }

    [HttpPost(ApiEndPointConstant.Wallet.Payment)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Trả tiền mặt cho payment ",
        Description = "Dùng cho Manager hoặc Driver xác nhận khách hàng trả tiền mặt")]
    public async Task<IActionResult> PaymentAsync([FromBody] WalletPaymentRequest walletPaymentRequest)
    {
        var result = await _walletService.DepositToWallet( walletPaymentRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Thanh toán thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Wallet.Get)]
    [ProducesResponseType(typeof(ApiResponse<GetWalletResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Lấy wallet theo customer id",
        Description = "Lấy wallet theo customer id")]
    public async Task<IActionResult> GetWalletByCustomerIdAsync([FromRoute] Guid id)
    {
        var result = await _walletService.CalculateWallet(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem ví thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }


}
