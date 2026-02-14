using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Claim;
using OmniChat.Infrastructure.Dtos.Responses.Claim;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

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

        [HttpGet(ApiEndPointConstant.ClaimEndPoint.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ClaimDetailResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Lấy tất cả Claim",
        Description = "Lấy thông tin của tất cả Claim")]
        public async Task<IActionResult> GetAllAsync()
        {
            var claims = await _claimService.GetAllClaim();

            return Ok(new ApiResponse<IEnumerable<ClaimDetailResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get All Claim Successfully",
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
                Message = "Create Claim Successfully",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpPut(ApiEndPointConstant.ClaimEndPoint.Update)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
      Summary = "Cập nhật Claim",
      Description = "Cập nhật thông tin Claim khi đang Pending")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateClaimRequest request)
        {
            var result = await _claimService.UpdateClaimInforAsync(id, request);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Update Claim Successfully",
                IsSuccess = true,
                Data = result
            });
        }

        [HttpPatch(ApiEndPointConstant.ClaimEndPoint.Approve)]
        [ProducesResponseType(typeof(ApiResponse<ClaimDetailResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
       Summary = "Duyệt Claim",
       Description = "Approve Claim khi đang Pending")]
        public async Task<IActionResult> ApproveAsync(Guid id)
        {
            var result = await _claimService.ApproveClaimAsync(id);

            return Ok(new ApiResponse<ClaimDetailResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Approve Claim Successfully",
                IsSuccess = true,
                Data = result
            });
        }


        [HttpPatch(ApiEndPointConstant.ClaimEndPoint.Reject)]
        [ProducesResponseType(typeof(ApiResponse<ClaimDetailResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Từ chối Claim",
        Description = "Reject Claim khi đang Pending")]
        public async Task<IActionResult> RejectAsync(Guid id)
        {
            var result = await _claimService.RejectClaimAsync(id);

            return Ok(new ApiResponse<ClaimDetailResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Reject Claim Successfully",
                IsSuccess = true,
                Data = result
            });
        }
    }
}
