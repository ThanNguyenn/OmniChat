using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Metadatas;

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

        /// Get support conversation by ID
        [HttpGet(ApiEndPointConstant.SupportConversationEndPoint.GetById)]
        [ProducesResponseType(typeof(GetAllSupportConversationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSupportConversationById([FromRoute] Guid id)
        {
            var result = await _supportConversationService.GetSupportConversationByIdAsync(id);
            return Ok(result);
        }

        /// Get all support conversations with pagination
        [HttpGet(ApiEndPointConstant.SupportConversationEndPoint.GetAllPagingByCustomerName)]
        [ProducesResponseType(typeof(PagingResponse<GetAllSupportConversationResponse>), StatusCodes.Status200OK)]
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
