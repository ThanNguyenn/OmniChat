using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Requests.FacebookOauthToken;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class FacebookOauthTokenController : BaseController<FacebookOauthTokenController>
    {
       private readonly IFacebookOAuthService _facebookOAuthService;

        public FacebookOauthTokenController(ILogger<FacebookOauthTokenController> logger,IFacebookOAuthService facebookOAuthService) : base(logger)
        {
            _facebookOAuthService = facebookOAuthService;
        }


        [HttpPost(ApiEndPointConstant.FacebookOAuthToken.Create)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Tạo Facebook OAuth Token",
        Description = "Tạo mới Facebook OAuth Access Token"
         )]
        public async Task<IActionResult> CreateNewFacebookOathTokenAsync(
        [FromBody] FacebookOauthTokenRequest request)
        {
            var result = await _facebookOAuthService
                .CreateNewFacebookTokenAsync(request);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Create Facebook OAuth Token successfully",
                Data = result
            });
        }

        [HttpPut(ApiEndPointConstant.FacebookOAuthToken.Update)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Cập nhật Facebook OAuth Token",
        Description = "Cập nhật access token mới"
         )]
        public async Task<IActionResult> UpdateFacebookOathTokenAsync(
             [FromRoute] Guid id,
             [FromBody] string newAccessToken)
        {
            var result = await _facebookOAuthService
                .UpdateFacebookTokenAsync(id, newAccessToken);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Update Facebook OAuth Token successfully",
                Data = result
            });
        }

       
        [HttpDelete(ApiEndPointConstant.FacebookOAuthToken.Delete)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Xóa Facebook Token",
            Description = "Soft delete Facebook OAuth Token"
        )]
        public async Task<IActionResult> DeleteFacebookOathTokenAsync(
         [FromRoute] Guid id)
        {
            var result = await _facebookOAuthService
                .DeleteFacebookTokenAsync(id);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Delete Facebook OAuth Token successfully",
                Data = result
            });
        }
    }
}
