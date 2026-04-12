using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.ClaimType;
using OmniChat.Infrastructure.Dtos.Responses.Notification;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace OmniChat.Api.Controllers
{
    [ApiController]
    public class NotificationController : BaseController<NotificationController>
    {
        private readonly INotificationService _notificationService;
        public NotificationController(ILogger<NotificationController> logger, INotificationService notificationService) : base(logger)
        {
            _notificationService = notificationService;
        }

        [HttpGet(ApiEndPointConstant.NotificationEndPoint.GetUnRead)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationResponse>>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Lấy tất cả các notification của staff ",
        Description = "Lấy tất cả các notification của staff bằng staff Id")]
        public async Task<IActionResult> GetNotificationByStaffIdAsync([FromRoute] Guid staffId)
        {
            var result = await _notificationService.GetNotificationsByStaffIdAsync(staffId);

            return Ok(new ApiResponse<IEnumerable<NotificationResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get All notification Successfully",
                IsSuccess = true,
                Data = result
            });
        }
    }
}
