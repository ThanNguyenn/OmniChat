using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Product;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatch;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers;

[ApiController]
[Route(ApiEndPointConstant.Product.Base)]
public class ProductController : BaseController<ProductController>
{
    private readonly IProductService _productService;

    public ProductController(ILogger<ProductController> logger, IProductService productService) : base(logger)
    {
        _productService = productService;
    }

    [HttpPost(ApiEndPointConstant.Product.Create)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
    Summary = "Tạo mới product",
    Description = "Tạo mới product")]
    public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest createProductRequest)
    {
        var result = await _productService.CreateProductAsync(createProductRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status201Created, "Product created successfully", result);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut(ApiEndPointConstant.Product.Update)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Cập nhật product",
        Description = "Cập nhật thông tin cơ bản của product theo id. Chỉ những field khác null trong request mới được cập nhật."
    )]

    public async Task<IActionResult> UpdateProduct([FromRoute]Guid id,[FromBody] UpdateProductRequest updateProductRequest)
    {
        var result = await _productService.UpdateProductAsync(id, updateProductRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Product updated successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPut(ApiEndPointConstant.Product.UpdateImage)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Cập nhật product",
        Description = "Cập nhật hình ảnh của product theo id."
    )]

    public async Task<IActionResult> UpdateProductImage([FromRoute] Guid id, [FromForm] UpdateProductImageRequest updateProductRequest)
    {
        var result = await _productService.UpdateProductImageAsync(id, updateProductRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Product updated successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpDelete(ApiEndPointConstant.Product.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Xóa product",
        Description = "Xóa product theo id. Sau khi xóa, product sẽ không còn hiển thị trong danh sách."
    )]

    public async Task<IActionResult> DeleteProduct([FromRoute]Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Product deleted successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Product.GetAll)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllProductsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Lấy danh sách product",
        Description = "Lấy danh sách product có phân trang, tìm kiếm theo từ khóa, và sắp xếp theo field chỉ định."
    )]  
    public async Task<IActionResult> GetAllProducts([FromQuery] string? search, int? pageNumber, int? pageSize, string? sortBy, bool? descending)
    {
        var result = await _productService.GetProductsAsync(
            search,
            pageNumber ?? 1, 
            pageSize ?? 10,
            sortBy ?? "id",
            descending ?? false
            );
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get all products successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Product.GetForCreateOrder)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllProductsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
    Summary = "Lấy danh sách product cho trang create order.",
    Description = "Lấy danh sách product cho trang create order đã gồm các chức năng filter."
    )]
    public async Task<IActionResult> GetAllProductsCreateOrder([FromQuery] GetAllProductsCreateOrderQueryRequest getAllProductsCreateOrderQueryRequest)
    {
        var result = await _productService.GetProductForCreateOrderByIdAsync(getAllProductsCreateOrderQueryRequest);      
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get all products successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Product.GetById)]
    [ProducesResponseType(typeof(ApiResponse<GetProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Lấy product theo id",
        Description = "Lấy thông tin chi tiết của product dựa trên id."
    )]
    public async Task<IActionResult> GetProductById([FromRoute]Guid id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get product by id successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPost(ApiEndPointConstant.Product.AddStock)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Thêm stock cho product",
        Description = "Thêm stock cho một hoặc nhiều product. Có thể thêm batch mới hoặc cập nhật số lượng của batch đã tồn tại."
    )]
    public async Task<IActionResult> AddStock([FromBody] IEnumerable<AddProductStockRequest> addProductStockRequests)
    {
        await _productService.AddStockAsync(addProductStockRequests);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Stock added successfully", true);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Product.GetProductBatches)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetProductBatchesResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Lấy danh sách product batch",
        Description = "Lấy danh sách product batch có phân trang, có filter lấy batch mới nhất."
    )]
    public async Task<IActionResult> GetProductBatches([FromRoute] Guid productId, [FromQuery]int? pageNumber, int? pageSize, bool? isNewest)
    {
        var result = await _productService.GetProductBatchesAsync(productId, isNewest, pageNumber ?? 1, pageSize ?? 20);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get product batches successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Product.Dashboard)]
    [ProducesResponseType(typeof(ApiResponse<InventoryDashboardResponse>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Inventory Dashboard",
        Description = "Thống kê tổng sản phẩm, sản phẩm sắp hết hàng và số brand trong kho")]
    public async Task<IActionResult> GetInventoryDashboardAsync()
    {
        var result = await _productService.GetInventoryDashboardAsync();

        return Ok(new ApiResponse<InventoryDashboardResponse>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Get Inventory Dashboard Successfully",
            IsSuccess = true,
            Data = result
        });
    }
}
