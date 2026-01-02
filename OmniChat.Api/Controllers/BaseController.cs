using Microsoft.AspNetCore.Mvc;

namespace OmniChat.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseController<T> : ControllerBase where T : BaseController<T>
{
    protected ILogger<T> _logger;

    public BaseController(ILogger<T> logger)
    {
        _logger = logger;
    }
}
