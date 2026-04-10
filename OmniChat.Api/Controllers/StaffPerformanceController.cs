using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Performance;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class StaffPerformanceController : BaseController<StaffPerformanceController>
    {
        private readonly IStaffPerformanceService _staffPerformanceService;
        public StaffPerformanceController(ILogger<StaffPerformanceController> logger, IStaffPerformanceService staffPerformanceService) : base(logger)
        {
            _staffPerformanceService = staffPerformanceService;
        }

        [HttpPost(ApiEndPointConstant.StaffPerformanceEndPoint.InitializePerformanceForStaff)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [SwaggerOperation(
       Summary = "Tạo mới Staff performance cho staff",
       Description = "Tạo mới Staff performance cho staff bằng Id nếu staff là nhân viên mới ")]

        public async Task<IActionResult> InitializePerformanceForStaffAsync(Guid staffId)
        {
            await _staffPerformanceService.InitializePerformanceForStaffAsync(staffId);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Initialize Performance For Staff Successfully",
                IsSuccess = true,
                Data = true
            });
        }

        [HttpGet(ApiEndPointConstant.StaffPerformanceEndPoint.GetMonthlyAverage)]
        [ProducesResponseType(typeof(ApiResponse<MonthlyAverageResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy tổng hợp hiệu suất trung bình của tất cả nhân viên theo năm của role Admin",
            Description = "Lấy tổng hợp hiệu suất trung bình của tất cả nhân viên trong năm được chỉ định"
            )]
        public async Task<IActionResult> GetMonthlyAverageAsync([FromQuery] int? year)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;
            var result = await _staffPerformanceService.GetMonthlyAverageAsync(targetYear);
            var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get monthly average successfully", result);
            return StatusCode(StatusCodes.Status200OK, response);
        }
    }
}
