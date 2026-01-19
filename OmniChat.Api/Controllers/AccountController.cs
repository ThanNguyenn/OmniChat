using Microsoft.AspNetCore.Mvc;
using OmniChat.Api.Constants;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Account;
using OmniChat.Infrastructure.Metadatas;
using Swashbuckle.AspNetCore.Annotations;

namespace OmniChat.Api.Controllers;

//[ApiController]
//[Route(ApiEndPointConstant.Account.Base)]
//public class AccountController : BaseController<AccountController>
//{
//    private readonly IAccountService _accountService;

//    public AccountController (ILogger<AccountController> logger, IAccountService accountService) : base(logger)
//    {
//        _accountService = accountService;
//    }

//    //[HttpPost(ApiEndPointConstant.Account.Create)]
//    //[ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status201Created)]
//    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
//    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
//    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
//    //[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
//    //[SwaggerOperation(
//    //    Summary = "Tạo tài khoản cho staff",
//    //    Description = "Dùng cho admin tạo tài khoản cho staff đã tồn tại. Username và Password được gen tự động")]
//    //public async Task<IActionResult> CreateAccountAsync([FromBody]CreateAccountRequest createAccountRequest)
//    //{
//    //    var result = await _accountService.CreateAccountAsync(createAccountRequest);
//    //    var response = ApiResponseBuilder.BuildResponse(StatusCodes.Status201Created, "Account created successfully", result);

//    //return StatusCode(StatusCodes.Status201Created, response);
//    //}




//}
