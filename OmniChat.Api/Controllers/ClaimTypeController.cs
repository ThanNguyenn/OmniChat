using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.ClaimType;
using OmniChat.Infrastructure.Dtos.Responses.ClaimType;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class ClaimTypeController : BaseController<ClaimTypeController>
    {
        private readonly IClaimTypeService _claimTypeService;

        public ClaimTypeController(ILogger<ClaimTypeController> logger, IClaimTypeService claimTypeService) : base(logger)
        {
            _claimTypeService = claimTypeService;
        }
        [HttpGet(ApiEndPointConstant.ClaimTypeEndPoint.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetClaimTypeResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Lấy tất cả các Claim",
        Description = "Lấy thông tin của tất cả các Claim")]
        public async Task<IActionResult> GetAllAsync()
        {
            var claimTypes = await _claimTypeService.GetAllTypeAsync();

            return Ok(new ApiResponse<IEnumerable<GetClaimTypeResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get All ClaimType Successfully",
                IsSuccess = true,
                Data = claimTypes
            });
        }

        [HttpPost(ApiEndPointConstant.ClaimTypeEndPoint.Create)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
        [SwaggerOperation(
        Summary = "Tạo Claim mới ",
        Description = "Tạo Claim mới")]
        public async Task<IActionResult> CreateNewClaimTypeAsync(
            [FromBody] ClaimTypeRequest typeRequest)
        {
            await _claimTypeService.CreateNewClaimTypeAsync(typeRequest);

            return StatusCode(StatusCodes.Status201Created, new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Create ClaimType Successfully",
                IsSuccess = true,
                Data = true
            });
        }

        [HttpPut(ApiEndPointConstant.ClaimTypeEndPoint.Update)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
        Summary = "Cập nhật Claim",
        Description = "Cập nhập Claim bằng Id")]
        public async Task<IActionResult> UpdateClaimTypeAsync(
            [FromRoute] Guid id,
            [FromBody] ClaimTypeRequest typeRequest)
        {
            await _claimTypeService.UpdateClaimTypeAsync(id, typeRequest);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Update ClaimType Successfully",
                IsSuccess = true,
                Data = true
            });
        }

        [HttpDelete(ApiEndPointConstant.ClaimTypeEndPoint.Delete)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(
        Summary = "Xóa Claim",
        Description = "Xóa Claim bằng Id")]
        public async Task<IActionResult> DeleteClaimTypeAsync(
            [FromRoute] Guid id)
        {
            await _claimTypeService.DeleteClaimTypeByIdAsync(id);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Delete ClaimType Successfully",
                IsSuccess = true,
                Data = true
            });
        }
    }
}
