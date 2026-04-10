using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.BackgroundJobs;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers;

[ApiController]
[Route(ApiEndPointConstant.Auth.Base)]
public class InvoiceController : BaseController<InvoiceController>
{
    private readonly IInvoiceService _invoiceService;
    private readonly InvoiceJobRunner _runner;
    public InvoiceController(ILogger<InvoiceController> logger, IInvoiceService invoiceService, InvoiceJobRunner runner) : base(logger)

    {
        _invoiceService = invoiceService;
        _runner = runner;
    }


    [HttpGet(ApiEndPointConstant.Invoice.TotalRevenue)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "TotalRevenue",
        Description = "TotalRevenue")]
    public async Task<IActionResult> TotalRevenueAsync([FromQuery] DateTime from, DateTime to)
    {
        var result = await _invoiceService.TotalIncomeByTime(from, to);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Total revenue calculated successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }


    [HttpGet(ApiEndPointConstant.Invoice.TotalUnpaid)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "TotalUnpaid",
        Description = "TotalUnpaid")]
    public async Task<IActionResult> TotalUnpaidAsync([FromQuery] DateTime from, DateTime to)
    {
        var result = await _invoiceService.TotalUnpaidAmountByTime(from, to);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Total unpaid calculated successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPost(ApiEndPointConstant.IntentType.Base + "run")]
    public async Task<IActionResult> Run([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        await _runner.RunAsync(from, to);
        return Ok();
    }

}
