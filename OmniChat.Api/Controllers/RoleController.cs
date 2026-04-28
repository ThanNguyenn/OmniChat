using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers;


[ApiController]
[Route(ApiEndPointConstant.Role.Base)]
public class RoleController : BaseController<RoleController>
{
    private readonly IRoleService _roleService;
    public RoleController(ILogger<RoleController> logger, IRoleService roleService) : base(logger)
    {
        _roleService = roleService;
    }

    [HttpPost(ApiEndPointConstant.Role.GetAll)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
    Summary = "Get list role",
    Description = "Get list role")]

    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleService.GetRolesAsync();
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem danh sách vai trò thành công", roles);
        return StatusCode(StatusCodes.Status200OK, response);
    }
}
