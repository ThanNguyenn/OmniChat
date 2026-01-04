using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Webhooks.Zalo.WebhookMessage;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    [Route(ApiEndPointConstant.Webhooks.Zalo)]
    public class ZaloWebhookController : Controller
    {
        private readonly IWebhookService _webhookService;

        public ZaloWebhookController(IWebhookService webhookService)
        {
            _webhookService = webhookService;
        }

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] ZaloWebhookEvent zaloEvent)
        {
            await _webhookService.ZaloWebhookAsync(zaloEvent);
            return Ok();
        }
    }
}
