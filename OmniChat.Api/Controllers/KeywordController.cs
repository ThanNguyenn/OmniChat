using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Keyword;
using OmniChat.Infrastructure.Dtos.Responses.Intent;
using OmniChat.Infrastructure.Dtos.Responses.Keyword;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers;

[ApiController]
[Route(ApiEndPointConstant.Keyword.Base)]
public class KeywordController : ControllerBase
{
    private readonly IKeywordService _keywordService;

    public KeywordController(IKeywordService keywordService)
    {
        _keywordService = keywordService;
    }

    [HttpPost("analyze-text")]
    public async Task<IActionResult> AnalyzeText([FromBody] string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return BadRequest("Message is empty");

        var result = await _keywordService.AnalyzeTextAsync(message);
        return Ok(result);
    }

    [HttpPost("predict-intent")]
    public async Task<ActionResult<PredictResponse>> PredictIntent([FromBody] string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return BadRequest("Message is empty");

        var result = await _keywordService.AnalyzeMessageWithKeywordsAsync(message);
        return Ok(result);
    }

    [HttpPost(ApiEndPointConstant.Keyword.Create)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
    Summary = "Tạo mới keyword",
    Description = "Tạo mới keyword")]
    public async Task<IActionResult> CreateKeyword([FromBody] CreateKeywordRequest createKeywordRequest)
    {
        var result = await _keywordService.CreateKeywordAsync(createKeywordRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status201Created, "Tạo từ khóa thành công", result);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut(ApiEndPointConstant.Keyword.Update)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update keyword",
        Description = "Update keyword")]
    public async Task<IActionResult> UpdateKeyword([FromRoute] Guid id,[FromBody] UpdateKeywordRequest updateKeywordRequest)
    {
        var result = await _keywordService.UpdateKeywordAsync(id, updateKeywordRequest);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Cập nhật từ khóa thành công ", result);
        return Ok(response);
    }

    [HttpDelete(ApiEndPointConstant.Keyword.Delete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Delete keyword",
        Description = "Delete keyword")]
    public async Task<IActionResult> DeleteKeyword([FromRoute] Guid id)
    {
        var result = await _keywordService.DeleteKeywordAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xóa từ khóa thành công", result);
        return Ok(response);
    }

    [HttpGet(ApiEndPointConstant.Keyword.GetAll)]
    [ProducesResponseType(typeof(ApiResponse<PagingResponse<GetAllKeywordsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Lấy list keyword",
        Description = "Lấy list keyword")]
    public async Task<IActionResult> GetAllKeywords([FromQuery] Guid? intentTypeId, string? search,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "createdate",
        bool descending = true)
    {
        var result = await _keywordService.GetAllKeywordsAsync(intentTypeId, search, pageNumber, pageSize, sortBy, descending);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem danh sách từ khóa thành công", result);
        return Ok(response);
    }

     [HttpGet(ApiEndPointConstant.Keyword.GetById)]
    [ProducesResponseType(typeof(ApiResponse<GetKeywordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Lấy  keyword theo id",
        Description = "Lấy  keyword theo id")]
    public async Task<IActionResult> GetKeywordById([FromRoute] Guid id)
    {
        var result = await _keywordService.GetKeywordAsync(id);
        var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status200OK, "Xem từ khóa thành công", result);
        return Ok(response);
    }
}
