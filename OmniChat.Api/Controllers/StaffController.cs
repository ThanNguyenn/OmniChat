using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Staff;
using OmniChat.Infrastructure.Dtos.Responses.Staff;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace OmniChat.Api.Controllers;

[ApiController]
[Route(ApiEndPointConstant.Staff.Base)]
public class StaffController : BaseController<StaffController>
{
    private readonly IStaffService _staffService;
    public StaffController(ILogger<StaffController> logger, IStaffService staffService) : base(logger)
    {
        _staffService = staffService;
    }

    [HttpPost(ApiEndPointConstant.Staff.Create)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Tạo thông tin staff mới",
        Description = "Dùng cho manager tạo thông tin cho staff mới. Tất cả field trong body là required")]
    public async Task<IActionResult> CreateStaffAsync([FromBody] CreateStaffRequest createStaffRequest)
    {
        var result = await _staffService.CreateStaffAsync(createStaffRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status201Created, "Account created successfully", result);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut(ApiEndPointConstant.Staff.Update)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Cập nhật thông tin staff",
        Description = "Dùng cho manager cập nhật thông tin cho staff đã được tạo. Tất cả các trường trong body là tùy chọn (nullable), chỉ những trường có giá trị khác null sẽ được cập nhật")]
    public async Task<IActionResult> UpdateStaffAsync([FromRoute] Guid staffId, [FromBody] UpdateStaffRequest updateStaffRequest)
    {
        var result = await _staffService.UpdateStaffAsync(staffId, updateStaffRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Account updated successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpDelete(ApiEndPointConstant.Staff.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Xóa thông tin staff",
        Description = "Dùng cho manager xóa thông tin cho staff đã được tạo.")]
    public async Task<IActionResult> DeleteStaffAsync([FromRoute] Guid staffId)
    {
        var result = await _staffService.DeleteStaffAsync(staffId);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Account deleted successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.Staff.GetAll)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetStaffsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Lấy list thông tin staff theo phòng ban",
        Description = "Dùng cho manager và admin lấy list thông tin staff theo phòng ban.")]
    public async Task<IActionResult> GetAllStaffsAsync([FromQuery][Required]Guid deparmentId, int? pageNumber, int? pageSize, string? sortBy, bool? descending)
    {
        var result = await _staffService.GetStaffsAsync(
            deparmentId,
            pageNumber ?? 1,
            pageSize ?? 20,
            sortBy ?? "id",
            descending ?? false
        );
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get staffs successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

}
