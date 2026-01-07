using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class SupportConversationController : Controller
    {
        private readonly ISupportConversationService _supportConversationService;

        public SupportConversationController(ISupportConversationService supportConversationService)
        {
            _supportConversationService = supportConversationService;
        }

        /// Get all support conversations with pagination
        [HttpGet(ApiEndPointConstant.SupportConversationEndPoint.GetAllPagingByCustomerName)]
        [ProducesResponseType(typeof(PagingResponse<GetAllSupportConversationResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy toàn bộ SupportConversation Paging",
            Description = "Lấy toàn bộ thông tin của SupportConversation có Paging và search theo CustomerName"
            )]
        public async Task<IActionResult> GetAllSupportConversationsPaging([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 20,[FromQuery] string? customerName = null)
        {
            var result = await _supportConversationService.SupportConversationByCustomerNamePagingAsync(
                pageNumber,
                pageSize,
                customerName);

            return Ok(result);
        }
    }
}
