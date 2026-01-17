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

        // Get all support conversations with pagination
        [HttpGet(ApiEndPointConstant.SupportConversationEndPoint.GetAllPagingByCustomerName)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllSupportConversationResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy toàn bộ SupportConversation Paging",
            Description = "Lấy toàn bộ thông tin của SupportConversation có Paging và search theo CustomerName"
            )]
        public async Task<IActionResult> GetAllSupportConversationsPaging([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? customerName = null)
        {
            var result = await _supportConversationService.SupportConversationByCustomerNamePagingAsync(
                pageNumber,
                pageSize,
                customerName);

            return Ok(result);
        }

        // Get support conversation detail by ID
        [HttpGet(ApiEndPointConstant.SupportConversationEndPoint.GetConversationDetail)]
        [ProducesResponseType(typeof(ApiResponse<SupportConversationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Lấy thông tin SupportConversation",
            Description = "Lấy tất cả tin nhắn của SupportConversation theo ID"
            )]
        public async Task<IActionResult> GetConversationDetailByIdAsync([FromRoute] Guid conversationId)
        {
            var conversationDetail = await _supportConversationService.GetConversationDetailByIdAsync(conversationId);

            return Ok(new ApiResponse<SupportConversationDetailResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get Conversation Detail Successfully",
                IsSuccess = true,
                Data = conversationDetail
            });
        }

        // Get staff conversation sidebar by staff ID
        [HttpGet(ApiEndPointConstant.SupportConversationEndPoint.StaffPendingSidebar)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<StaffConversationSideBarResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy sidebar cuộc trò chuyện đang chờ của nhân viên hỗ trợ",
            Description = "Lấy sidebar cuộc trò chuyện đang chờ của nhân viên hỗ trợ theo StaffId"
            )]
        public async Task<IActionResult> GetStaffConversationSidebarAsync([FromRoute] Guid staffId)
        {
            var sidebarConversations = await _supportConversationService.GetStaffConversationSideBarAsync(staffId);
            return Ok(new ApiResponse<IEnumerable<StaffConversationSideBarResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get Staff Conversation Sidebar Successfully",
                IsSuccess = true,
                Data = sidebarConversations
            });
        } 
    }
}
