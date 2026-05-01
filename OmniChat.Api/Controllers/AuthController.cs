using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Auth;
using OmniChat.Infrastructure.Dtos.Responses.Auth;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers;

[ApiController]
[Route(ApiEndPointConstant.Auth.Base)]
public class AuthController : BaseController<AuthController>
{
    private readonly IAuthService _authService;

    public AuthController(ILogger<AuthController> logger, IAuthService authService) : base(logger)
    {
        _authService = authService;
    }

    [HttpPost(ApiEndPointConstant.Auth.Login)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
    Summary = "Đăng nhập bằng tài khoản của app",
    Description = "Đăng nhập bằng tài khoản của app sử dụng email hoặc username và mật khẩu, nhận role, access token và refresh token trả về.\n" +
        "Default password: Omnichat@0294")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var result = await _authService.LoginAsync(loginRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Đăng nhập thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [Authorize]
    [HttpPost(ApiEndPointConstant.Auth.Logout)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
    Summary = "Logout tài khoản của app",
    Description = "Logout tài khoản của app")]
    public async Task<IActionResult> Logout()
    {
        var result = await _authService.LogoutAsync();
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Đăng xuất thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [Authorize]
    [HttpPost(ApiEndPointConstant.Auth.ChangePassword)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
    Summary = "Thay đổi password",
    Description = "Dùng cho staff thay đổi password nếu nhớ mật khẩu cũ.")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordResquest changePasswordRequest)
    {
        var result = await _authService.ChangePasswordAsync(changePasswordRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Thay đổi mật khẩu thành công", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPost(ApiEndPointConstant.Auth.RefreshToken)]
    [ProducesResponseType(typeof(ApiResponse<RefreshAccessTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Refresh Access Token",
        Description = "Dùng Refresh Token được cấp để nhận Access Token mới.")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshAccessTokenRequest refreshAccessTokenRequest)
    {
        var result = await _authService.RefreshAccessToken(refreshAccessTokenRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Làm mới access token thành công.", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }


}
