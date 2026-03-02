using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;
using static OmniChat.Api.Constants.ApiEndPointConstant;

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
            Description = "Lấy sidebar cuộc trò chuyện đang chờ của nhân viên hỗ trợ theo StaffId và tên provider"
            )]
        public async Task<IActionResult> GetStaffConversationSidebarAsync([FromRoute] Guid staffId, [FromQuery] string? providerName = null)
        {
            var sidebarConversations = await _supportConversationService.GetStaffConversationSideBarAsync(staffId, providerName);
            return Ok(new ApiResponse<IEnumerable<StaffConversationSideBarResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get Staff Conversation Sidebar Successfully",
                IsSuccess = true,
                Data = sidebarConversations
            });
        }

        [HttpGet(ApiEndPointConstant.SupportConversationEndPoint.GetCompletedConversationDetail)]
        [ProducesResponseType(typeof(ApiResponse<SupportConversationDetailResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
      Summary = "Lấy lịch sử cuộc trò chuyện hoàn thành của khách hàng",
      Description = "Lấy lịch sử cuộc trò chuyện hoàn thành trước đó của khách hàng theo conversationId"
        )]
        public async Task<IActionResult> GetCustomerConversationHistoryAsync([FromRoute] Guid conversationId)
        {
            var conversation = await _supportConversationService.GetCustomerConversationHistoryAsync(conversationId);

            return Ok(new ApiResponse<SupportConversationDetailResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get Completed Conversation Detail Successfully",
                IsSuccess = true,
                Data = conversation
            });
        }

        [HttpGet(ApiEndPointConstant.SupportConversationEndPoint.CustomerCompleteConversationHistory)]
        [ProducesResponseType(typeof(ApiResponse<List<CompleteSupportConversationHistoryResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy lịch sử cuộc trò chuyện đã hoàn thành của khách hàng",
            Description = "Lấy toàn bộ lịch sử cuộc trò chuyện support đã hoàn thành trước đó của khách hàng theo CustomerId"
            )]
        public async Task<IActionResult> GetCustomerCompleteConversationHistoryAsync([FromRoute] Guid customerId)
        {
            if (customerId == Guid.Empty)
                return BadRequest("CustomerId is invalid");

            var conversations = await _supportConversationService
                .GetCustomerCompleteSupportConversationHistoryAsync(customerId);

            return Ok(new ApiResponse<List<CompleteSupportConversationHistoryResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get Customer Complete Conversation History Successfully",
                IsSuccess = true,
                Data = conversations
            });
        }
    }
}
