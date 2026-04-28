using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class SupportConversationController : BaseController<SupportConversationController>
    {
        private readonly ISupportConversationService _supportConversationService;

        public SupportConversationController(ILogger<SupportConversationController> logger, ISupportConversationService supportConversationService) : base(logger)
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
                Message = "Xem cuộc trò chuyện thành công",
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
                Message = "Xem danh sách cuộc trò chuyện thành công",
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
                Message = "Xem chi tiết lịch sử cuộc trò chuyện thành công",
                IsSuccess = true,
                Data = conversation
            });
        }

        [HttpGet(ApiEndPointConstant.SupportConversationEndPoint.CustomerCompleteConversationHistory)]
        [ProducesResponseType(typeof(ApiResponse<List<CompleteSupportConversationHistoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Lấy lịch sử cuộc trò chuyện đã hoàn thành của khách hàng",
            Description = "Lấy toàn bộ lịch sử cuộc trò chuyện support đã hoàn thành trước đó của khách hàng theo CustomerId"
            )]
        public async Task<IActionResult> GetCustomerCompleteConversationHistoryAsync([FromRoute] Guid customerId)
        {
            if (customerId == Guid.Empty)
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Mã khách hàng không hợp lệ", 
                    IsSuccess = false
                });

            var conversations = await _supportConversationService
                .GetCustomerCompleteSupportConversationHistoryAsync(customerId);

            return Ok(new ApiResponse<List<CompleteSupportConversationHistoryResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Xem lịch sử cuộc trò chuyện thành công",
                IsSuccess = true,
                Data = conversations
            });
        }

        [HttpPatch(ApiEndPointConstant.SupportConversationEndPoint.CompleteConversation)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Hoàn thành conversation",
            Description = "Hoàn thành conversation bằng conversaiton id"
            )]
        public async Task<IActionResult> CompleteConversationAsync(Guid id)
        {
            var result = await _supportConversationService.CompleteConversationAsync(id);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Hoàn thành Conversation thành công",
                IsSuccess = true,
                Data = result
            });
        }


        [HttpGet(ApiEndPointConstant.SupportConversationEndPoint.GetStaffConversationsForSelect)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<StaffConversationResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Lấy danh sách cuộc trò chuyện của nhân viên để chọn",
        Description = "Lấy danh sách cuộc trò chuyện của nhân viên theo staffId"
          )]
        public async Task<IActionResult> GetConversationsForClaimSelect(
        Guid staffId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
        {
            
            var result = await _supportConversationService.GetStaffConversationAsync(staffId, pageNumber, pageSize);
            return Ok(new ApiResponse<PagingResponse<StaffConversationResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Xem danh sách cuộc trò chuyện thành công",
                IsSuccess = true,
                Data = result
            });
        }
    }
}
