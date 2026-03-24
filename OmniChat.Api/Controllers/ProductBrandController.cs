using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Brand;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers;

[ApiController]
[Route(ApiEndPointConstant.Brand.Base)]
public class ProductBrandController : BaseController<ProductBrandController>
{
    private readonly IProductBrandService _productBrandService;

    public ProductBrandController(ILogger<ProductBrandController> logger, IProductBrandService productBrandService) : base(logger)
    {
        _productBrandService = productBrandService;
    }


    [HttpGet(ApiEndPointConstant.Brand.GetAll)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetAllBrandsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
       Summary = "Lấy danh sách brand",
       Description = "Lấy danh sách tất cả product brand."
   )]
    public async Task<IActionResult> GetAllProductBrands()
    {
        var result = await _productBrandService.GetAllProductBrandsAsync();
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Product brands retrieved successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }
}

