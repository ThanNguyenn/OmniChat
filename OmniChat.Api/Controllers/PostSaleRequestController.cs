
using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.PostSaleRequest;
using OmniChat.Infrastructure.Dtos.Responses.PostSaleRequest;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers;

[ApiController]
[Route(ApiEndPointConstant.PostSaleRequest.Base)]
public class PostSaleRequestController : BaseController<PostSaleRequestController>
{
    private readonly IPostSaleRequestService _postSaleRequestService;

    public PostSaleRequestController(ILogger<PostSaleRequestController> logger, IPostSaleRequestService postSaleRequestService) : base(logger)
    {
        _postSaleRequestService = postSaleRequestService;
    }

    [HttpGet(ApiEndPointConstant.PostSaleRequest.GetAll)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetPostSaleRequestsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
       Summary = "Lấy danh sách post sale request có paging",
       Description = "Lấy danh sách post sale request có paging."
    )]
    public async Task<IActionResult> GetPostSaleRequests([FromQuery] int? pageNumber, int? pageSize, string? sortBy, bool? descending)
    {
        var result = await _postSaleRequestService.GetPostSaleRequestsAsync(pageNumber ?? 1, pageSize ?? 20, sortBy ?? "createddate", descending ?? true);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get post sale requests successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpGet(ApiEndPointConstant.PostSaleRequest.GetById)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetPostSaleRequestsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
       Summary = "Lấy post sale request theo id",
       Description = "Lấy post sale request theo id."
    )]
    public async Task<IActionResult> GetPostSaleRequestById([FromRoute] Guid id)
    {
        var result = await _postSaleRequestService.GetPostSaleRequestByIdAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Get post sale request successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPost(ApiEndPointConstant.PostSaleRequest.Create)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
       Summary = "Tạo post sale request",
       Description = "Tạo post sale request."
    )]
    public async Task<IActionResult> CreatePostSaleRequest([FromBody] CreatePostSaleRequestRequest request)
    {
        var result = await _postSaleRequestService.CreatePostSaleRequestAsync(request);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status201Created, "Create post sale request successfully", result);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    //[HttpPut(ApiEndPointConstant.PostSaleRequest.Update)]
    //[ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    //[SwaggerOperation(
    //   Summary = "Update post sale request",
    //   Description = "Update post sale request."
    //)]
    //public async Task<IActionResult> UpdatePostSaleRequest([FromRoute] Guid id, [FromBody] UpdatePostSaleRequestRequest request)
    //{
    //    var result = await _postSaleRequestService.UpdatePostSaleRequestAsync(id, request);
    //    var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Update post sale request successfully", result);
    //    return StatusCode(StatusCodes.Status200OK, response);
    //}

    //[HttpDelete(ApiEndPointConstant.PostSaleRequest.Delete)]
    //[ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    //[SwaggerOperation(
    //   Summary = "Delete post sale request",
    //   Description = "Delete post sale request."
    //)]
    //public async Task<IActionResult> DeletePostSaleRequest([FromRoute] Guid id)
    //{
    //    var result = await _postSaleRequestService.DeletePostSaleRequestAsync(id);
    //    var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Delete post sale request successfully", result);
    //    return StatusCode(StatusCodes.Status200OK, response);
    //}

    [HttpPost(ApiEndPointConstant.PostSaleRequest.Approve)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
       Summary = "Approve post sale request",
       Description = "Approve post sale request."
    )]
    public async Task<IActionResult> ApprovePostSaleRequest([FromRoute] Guid id)
    {
        var result = await _postSaleRequestService.AcceptPostSaleRequestAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Approve post sale request successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }

    [HttpPost(ApiEndPointConstant.PostSaleRequest.Reject)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
       Summary = "Reject post sale request",
       Description = "Reject post sale request."
    )]
    public async Task<IActionResult> RejectPostSaleRequest([FromRoute] Guid id)
    {
        var result = await _postSaleRequestService.RejectPostSaleRequestAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Reject post sale request successfully", result);
        return StatusCode(StatusCodes.Status200OK, response);
    }
}


