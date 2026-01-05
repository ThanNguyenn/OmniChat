using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Webhooks.Facebook.WebhookMessage;
using OmniChat.Application.Webhooks.Zalo.WebhookMessage;
using OmniChat.Infrastructure.Metadatas;

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
        [HttpPost(ApiEndPointConstant.Webhooks.ZaloWebhook)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ZaloWebhookAsync(
            [FromBody] ZaloWebhookEvent zaloEvent)
        {
            if (zaloEvent == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid Zalo webhook payload",
                    Reason = "Payload is null",
                    IsSuccess = false,
                    Data = null
                });
            }

            await _webhookService.ZaloWebhookAsync(zaloEvent);

            return Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Zalo webhook processed successfully",
                IsSuccess = true,
                Data = null
            });
        }

        // POST /api/v1/webhooks/facebook
        [HttpPost(ApiEndPointConstant.Webhooks.FacebookWebhook)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FacebookWebhookAsync(
            [FromBody] FaceBookWebhookPayload payload)
        {
            if (payload == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Invalid Facebook webhook payload",
                    Reason = "Payload is null",
                    IsSuccess = false,
                    Data = null
                });
            }

            await _webhookService.FacebookWebhookAsync(payload);

            return Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Facebook webhook processed successfully",
                IsSuccess = true,
                Data = null
            });
        }
    }
}
