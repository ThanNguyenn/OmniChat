using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.BackgroundJobs;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Invoice;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers;

[ApiController]
[Route(ApiEndPointConstant.Auth.Base)]
public class InvoiceController : BaseController<InvoiceController>
{
    private readonly IInvoiceService _invoiceService;
    private readonly InvoiceJobRunner _runner;
    private readonly ISheetExportService _sheetExportService;
    private readonly IWebHostEnvironment _hostingEnvironment;
    public InvoiceController(ILogger<InvoiceController> logger, IInvoiceService invoiceService, InvoiceJobRunner runner, ISheetExportService sheetExportService, IWebHostEnvironment hostingEnvironment) : base(logger)

    {
        _invoiceService = invoiceService;
        _runner = runner;
        _sheetExportService = sheetExportService;
        _hostingEnvironment = hostingEnvironment;
    }


    [HttpGet(ApiEndPointConstant.Invoice.TotalRevenue)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DashBoardInvoiceByYearResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "TotalRevenue",
        Description = "TotalRevenue. Input yyyy để lấy 12 tháng hoặc mm/yyyy để lấy theo tháng ")]
    public async Task<IActionResult> TotalRevenueAsync([FromQuery] string period)
    {
        var result = await _invoiceService.GetTotalIncomeAsync(period);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Total revenue calculated successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }


    [HttpGet(ApiEndPointConstant.Invoice.TotalUnpaid)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DashBoardInvoiceByYearResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "TotalUnpaid",
        Description = "TotalUnpaid. Input yyyy để lấy 12 tháng hoặc mm/yyyy để lấy theo tháng ")]
    public async Task<IActionResult> TotalUnpaidAsync([FromQuery] string period)
    {
        var result = await _invoiceService.GetTotalUnpaidAsync(period);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Total unpaid calculated successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }


    [HttpPost(ApiEndPointConstant.Invoice.Base + "run")]
    public async Task<IActionResult> Run([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        await _runner.RunAsync(from, to);
        return Ok();
    }

    [HttpGet(ApiEndPointConstant.Invoice.ExportToExcel)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]

    public async Task<IActionResult> ExportToExcel([FromRoute] Guid id)
    {
        var templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "templates", "SheetTemplate.xlsx");
        var fileContent = await _sheetExportService.ExportInvoiceToExcelAsync(id, templatePath);
        return File(
            fileContent.content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileContent.filename
        );
    }

    [HttpGet(ApiEndPointConstant.Invoice.GetAll)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetInvoicesResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Lấy danh sách Invoice",
        Description = "Lấy danh sách Invoice có phân trang, và sắp xếp theo field chỉ định."
    )]
    public async Task<IActionResult> GetAllProducts([FromQuery] Guid? invoiceId, InvoiceStatus? status, int? pageNumber, int? pageSize, string? sortBy, bool? descending)
    {
        var result = await _invoiceService.GetInvoicesAsync(
            invoiceId,
            status,
            pageNumber ?? 1,
            pageSize ?? 10,
            sortBy ?? "id",
            descending ?? false
            );
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get all products successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Invoice.GetById)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetInvoicesResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Lấy Invoice theo Id",
        Description = "Lấy Invoice theo Id."
    )]
    public async Task<IActionResult> GetInvoiceById([FromRoute] Guid id)
    {
        var result = await _invoiceService.GetInvoiceAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get invoice by id successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);

    }
}