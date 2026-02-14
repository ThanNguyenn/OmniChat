using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.InstagramOauthToken;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class InstagramOauthTokenController : BaseController<InstagramOauthTokenController>
    {
        private readonly IInstagramOAuthService _instagramOAuthService;
        public InstagramOauthTokenController(ILogger<InstagramOauthTokenController> logger, IInstagramOAuthService instagramOAuthService) : base(logger)
        {
            _instagramOAuthService = instagramOAuthService;
        }

        [HttpPost(ApiEndPointConstant.InstagramOAuthToken.Create)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Tạo Instagram OAuth Token",
        Description = "Tạo mới Instagram OAuth Access Token"
         )]
        public async Task<IActionResult> CreateNewInstagramOathTokenAsync(
        [FromBody] InstagramOauthTokenRequest request)
        {
            var result = await _instagramOAuthService
                .CreateInstagramOauthTokenAsync(request);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Create Instagram OAuth Token successfully",
                Data = result
            });
        }

        [HttpPut(ApiEndPointConstant.InstagramOAuthToken.Update)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
       Summary = "Cập nhật Instagram OAuth Token",
       Description = "Cập nhật access token mới"
        )]
        public async Task<IActionResult> UpdateInstagramOathTokenAsync(
            [FromRoute] Guid id,
            [FromBody] string newAccessToken)
        {
            var result = await _instagramOAuthService
                .UpdateInstagramOathTokenAsync(id, newAccessToken);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Update Instagram OAuth Token successfully",
                Data = result
            });
        }

        [HttpDelete(ApiEndPointConstant.InstagramOAuthToken.Delete)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
           Summary = "Xóa Instagram Token",
           Description = "Soft delete Instagram OAuth Token"
       )]
        public async Task<IActionResult> DeleteInstagramOathTokenAsync(
        [FromRoute] Guid id)
        {
            var result = await _instagramOAuthService
                .DeleteInstagramTokenAsync(id);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Delete Instagram OAuth Token successfully",
                Data = result
            });
        }
    }
}
