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

        [HttpGet(ApiEndPointConstant.StaffPerformanceEndPoint.GetTotalAverage)]
        [ProducesResponseType(typeof(ApiResponse<TotalAverageResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Lấy tổng hợp hiệu suất trung bình của tất cả nhân viên của role Admin",
            Description = "Lấy tổng hợp hiệu suất trung bình của tất cả nhân viên trong khoảng thời gian được chỉ định"
            )]
        public async Task<IActionResult> GetTotalAverageAsync([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var result = await _staffPerformanceService.GetTotalAverageAsync(fromDate, toDate);
            return Ok(new ApiResponse<TotalAverageResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get Total Average Successfully",
                IsSuccess = true,
                Data = result
            });
        }
    }
}
