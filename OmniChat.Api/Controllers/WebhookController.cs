using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Webhooks.Facebook.WebhookMessage;
using OmniChat.Application.Webhooks.Instagram.InstagramMessage;
using OmniChat.Application.Webhooks.Zalo.WebhookMessage;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IWebhookService _webhookService;

        public WebhookController(IWebhookService webhookService)
        {
            _webhookService = webhookService;
        }

        // POST /api/v1/webhooks/zalo
        //[HttpPost(ApiEndPointConstant.Webhooks.ZaloWebhook)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]

        //[SwaggerOperation(
        //Summary = "Webhook set up của zalo",
        //Description = "Webhook hứng evernt từ phía zalo "
        //)]
        //public async Task<IActionResult> ZaloWebhookAsync(
        //    [FromBody] ZaloWebhookEvent zaloEvent)
        //{
        //   await _webhookService.ZaloWebhookAsync(zaloEvent);

        //    return Ok(new ApiResponse<object>
        //    {
        //        StatusCode = StatusCodes.Status200OK,
        //        Message = "Zalo webhook processed successfully",
        //        IsSuccess = true,
        //        Data = null
        //    });
        //}

        // POST /api/v1/webhooks/facebook
        [HttpPost(ApiEndPointConstant.Webhooks.FacebookWebhook)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Webhook set up của Facebook",
        Description = "Webhook hứng evernt từ phía Facebook "
        )]
        public async Task<IActionResult> FacebookWebhookAsync(
            [FromBody] FaceBookWebhookPayload payload)
        {

            await _webhookService.FacebookWebhookAsync(payload);

            return Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Facebook webhook processed successfully",
                IsSuccess = true,
                Data = null
            });
        }

        [HttpGet(ApiEndPointConstant.Webhooks.FacebookWebhook)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Webhook set up verify của Facebook",
        Description = "Webhook verify hứng event từ phía Facebook "
        )]
        public async Task<IActionResult> VerifyFacebookAsync(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge)
        {
            var isValid = await _webhookService.VerifyFacebookWebhook(mode, token);

            if (!isValid)
                return Forbid();

            return Content(challenge, "text/plain");
        }

        // POST /api/v1/webhooks/instagram
        [HttpPost(ApiEndPointConstant.Webhooks.InstagramWebhook)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> InstagramWebhookAsync(
            [FromBody] InstagramWebhookPayload payload)
        {
            await _webhookService.InstagramWebhookAsync(payload);

            return Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Instagram webhook processed successfully",
                IsSuccess = true,
                Data = null
            });
        }
        // GET: verify webhook
        [HttpGet(ApiEndPointConstant.Webhooks.InstagramWebhook)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyInstagramAsync(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge)
        {
            var isValid = await _webhookService.VerifyInstagramWebhook(mode, token);

            if (!isValid)
                return Forbid();

            return Content(challenge, "text/plain");
        }
    }
}
