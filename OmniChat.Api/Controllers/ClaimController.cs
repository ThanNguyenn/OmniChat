using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Claim;
using OmniChat.Infrastructure.Dtos.Responses.Claim;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace OmniChat.Api.Controllers
{

    [ApiController]
    public class ClaimController : BaseController<ClaimController>
    {
        private readonly IClaimService _claimService;

        public ClaimController(ILogger<ClaimController> logger, IClaimService claimService) : base(logger)
        {
            _claimService = claimService;
        }

        [HttpGet(ApiEndPointConstant.ClaimEndPoint.GetHistory)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<ClaimDetailResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy lịch sử Claim",
            Description = "Lấy danh sách Claim đã xử lý (không bao gồm Pending)")]
        public async Task<IActionResult> GetClaimHistoryAsync(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
        {
            var claims = await _claimService.GetClaimHistoryAsync(pageIndex, pageSize);

            return Ok(new ApiResponse<PagingResponse<ClaimDetailResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy lịch sử khiếu nại thành công",
                IsSuccess = true,
                Data = claims
            });
        }

        [HttpGet(ApiEndPointConstant.ClaimEndPoint.GetPending)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<ClaimDetailResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Lấy danh sách Claim đang chờ xử lý",
        Description = "Lấy các Claim có trạng thái Pending")]
        public async Task<IActionResult> GetPendingClaimsAsync(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
        {
            var claims = await _claimService.GetPendingClaimAsync(pageIndex, pageSize);

            return Ok(new ApiResponse<PagingResponse<ClaimDetailResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy danh sách khiếu nại đang chờ xử lý thành công",
                IsSuccess = true,
                Data = claims
            });
        }


        [HttpPost(ApiEndPointConstant.ClaimEndPoint.Create)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
       Summary = "Tạo mới Claim",
       Description = "Tạo mới một Claim")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateClaimRequest request)
        {
            var result = await _claimService.CreateClaimAsync(request);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Tạo khiếu nại thành công",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpGet(ApiEndPointConstant.ClaimEndPoint.Dashboard)]
        [ProducesResponseType(typeof(ApiResponse<ClaimDashboardResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Claim Dashboard",
        Description = "Thống kê số lượng claim theo trạng thái")]
        public async Task<IActionResult> GetClaimDashboardAsync()
        {
            var result = await _claimService.GetClaimDashboardAsync();

            return Ok(new ApiResponse<ClaimDashboardResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy dữ liệu dashboard thành công",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpPut(ApiEndPointConstant.ClaimEndPoint.Update)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
      Summary = "Cập nhật Claim",
      Description = "Cập nhật thông tin Claim khi đang Pending")]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateClaimRequest request)
        {
            var result = await _claimService.UpdateClaimInforAsync(id, request);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Cập nhật khiếu nại thành công",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpPatch(ApiEndPointConstant.ClaimEndPoint.Approve)]
        [ProducesResponseType(typeof(ApiResponse<ClaimDetailResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
       Summary = "Duyệt Claim",
       Description = "Approve Claim khi đang Pending")]
        public async Task<IActionResult> ApproveAsync([FromRoute] Guid id)
        {
            var result = await _claimService.ApproveClaimAsync(id);

            return Ok(new ApiResponse<ClaimDetailResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Phê duyệt khiếu nại thành công",
                IsSuccess = true,
                Data = result
            });
        }


        [HttpPatch(ApiEndPointConstant.ClaimEndPoint.Reject)]
        [ProducesResponseType(typeof(ApiResponse<ClaimDetailResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Từ chối Claim",
        Description = "Reject Claim khi đang Pending")]
        public async Task<IActionResult> RejectAsync([FromRoute] Guid id)
        {
            var result = await _claimService.RejectClaimAsync(id);

            return Ok(new ApiResponse<ClaimDetailResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Từ chối khiếu nại thành công",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpGet(ApiEndPointConstant.ClaimEndPoint.GetByStaffId)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<ClaimDetailResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
        Summary = "Lấy Claim theo StaffId",
        Description = "Lấy tất cả Claim của một nhân viên dựa trên StaffId"
        )]
        public async Task<IActionResult> GetByStaffIdAsync([FromRoute] Guid staffId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var claims = await _claimService.GetClaimsByStaffIdAsync(staffId, pageIndex, pageSize);
            return Ok(new ApiResponse<PagingResponse<ClaimDetailResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy danh sách khiếu nại của nhân viên thành công",
                IsSuccess = true,
                Data = claims
            });
        }

        [HttpPut(ApiEndPointConstant.ClaimEndPoint.ApproveReAssign)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            summary: "Gán Conversation cho new Staff ",
            description: "Gán lại một Conversation cho nhân viên khác dựa trên ConversationId và NewStaffId"
            )]
        public async Task<IActionResult> ReAssignStaffAsync(
        [FromRoute] Guid claimId,
        [FromRoute] Guid conversationId,
        [FromRoute] Guid newStaffId)
        {
            await _claimService.ReAssignStaffAsync(claimId, newStaffId, conversationId);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Phê duyệt và chuyển giao nhân viên thành công",
                IsSuccess = true,
                Data = true
            });
        }

        [HttpPut(ApiEndPointConstant.ClaimEndPoint.RejectReAssign)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Từ chối yêu cầu chuyển giao công việc",
        Description = "Manager từ chối yêu cầu, đưa hội thoại về trạng thái làm việc cho nhân viên cũ"
        )]
        public async Task<IActionResult> RejectReassignClaimAsync(
        [FromRoute] Guid id,
        [FromRoute] Guid managerId) 
        {

            await _claimService.RejectReassignClaimAsync(id, managerId);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Đã từ chối yêu cầu chuyển giao",
                IsSuccess = true,
                Data = true
            });
        }


        [HttpGet(ApiEndPointConstant.ClaimEndPoint.GetPendingChangeTask)]
        [ProducesResponseType(typeof(ApiResponse<PagingResponse<ClaimDetailResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy danh sách ChangeTask Claim đang chờ xử lý",
            Description = "Lấy tất cả ChangeTask Claim đang chờ xử lý"
        )]
        public async Task<IActionResult> GetPendingChangeTaskAsync([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var claims = await _claimService.GetPendingChangeTask(pageIndex, pageSize);
            return Ok(new ApiResponse<PagingResponse<ClaimDetailResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy danh sách yêu cầu đổi việc đang chờ xử lý thành công",
                IsSuccess = true,
                Data = claims
            });
        }
    }
}