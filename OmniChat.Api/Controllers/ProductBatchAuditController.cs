using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.ProductBatchAudit;
using OmniChat.Infrastructure.Dtos.Responses.PostSaleRequest;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatchAudit;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;
using Action = OmniChat.Infrastructure.Models.Action;
namespace OmniChat.Api.Controllers;


[ApiController]
[Route(ApiEndPointConstant.BatchAudit.Base)]
public class ProductBatchAuditController  : BaseController<ProductBatchAuditController>
{
    private readonly IProductBatchAuditService _productBatchAuditService;

    public ProductBatchAuditController(ILogger<ProductBatchAuditController> logger, IProductBatchAuditService productBatchAuditService) : base(logger)
    {
        _productBatchAuditService = productBatchAuditService;
    }

    [HttpGet(ApiEndPointConstant.BatchAudit.GetAll)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllAuditResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
       Summary = "Xem danh sách batch audit có paging",
       Description = "Xem danh sách batch audit có paging"
    )]
    public async Task<IActionResult> GetProductBatchAudits([FromQuery] int? pageNumber, int? pageSize, string? sortBy, bool? descending, Guid? productId,Guid? productBatchId, Action? action)
     {
         var result = await _productBatchAuditService.GetAllAuditAsync(productId, productBatchId, action,  pageNumber ?? 1, pageSize ?? 20, sortBy ?? "createddate", descending ?? true);
         var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem danh sách batch audit thành công", result);
         return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.BatchAudit.GetByProductId)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllAuditResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
       Summary = "Xem danh sách batch audit theo product id",
       Description = "Xem danh sách batch audit theo product id"
    )]
    public async Task<IActionResult> GetProductBatchAuditsByProductId([FromRoute] Guid productId, [FromQuery] int? pageNumber, int? pageSize, string? sortBy, bool? descending, Action? action)
    {
        var result = await _productBatchAuditService.GetAllAuditAsync(productId, null, action, pageNumber ?? 1, pageSize ?? 20, sortBy ?? "createddate", descending ?? true);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem danh sách batch audit theo id sản phẩm thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.BatchAudit.GetByBatchId)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllAuditResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
       Summary = "Xem danh sách batch audit theo productBatch id",
       Description = "Xem danh sách batch audit theo productBatch id"
    )]
    public async Task<IActionResult> GetProductBatchAuditsByBatchId([FromRoute] Guid productBatchId, [FromQuery] int? pageNumber, int? pageSize, string? sortBy, bool? descending, Action? action)
    {
        var result = await _productBatchAuditService.GetAllAuditAsync(null, productBatchId, action, pageNumber ?? 1, pageSize ?? 20, sortBy ?? "createddate", descending ?? true);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem danh sách batch audit theo id lô sản phẩm thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.BatchAudit.GetDetailByBatchId)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllAuditResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
       Summary = "Xem chi tiết batch audit theo productBatch id",
       Description = "Xem chi tiết batch audit theo productBatch id"
    )]
    public async Task<IActionResult> GetProductBatchAuditDetailByBatchId([FromRoute] Guid productBatchId)
    {
        var result = await _productBatchAuditService.GetDetailByBatchIdAsync(productBatchId);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem chi tiết batch audit theo id lô sản phẩm thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPut(ApiEndPointConstant.BatchAudit.Update)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
       Summary = "Cập nhật batch audit",
       Description = "Cập nhật batch audit"
    )]
    public async Task<IActionResult> UpdateProductBatchAudit([FromRoute] Guid id, [FromBody] UpdateBatchAuditRequest request)
    {
        var result = await _productBatchAuditService.UpdateBatchAuditAsync(id, request);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Cập nhật batch audit thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }


    [HttpDelete(ApiEndPointConstant.BatchAudit.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
       Summary = "Xóa batch audit",
       Description = "Xóa batch audit"
    )]
    public async Task<IActionResult> DeleteProductBatchAudit([FromRoute] Guid id)
    {
        var result = await _productBatchAuditService.DeleteBatchAuditAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xóa batch audit thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }
}
