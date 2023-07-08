using CentCom.API.Models;
using CentCom.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CentCom.API.Controllers;

[Produces("application/json")]
[ApiController]
public class StatusController : ControllerBase
{
    private readonly IAppStatusService _status;

    public StatusController(IAppStatusService status)
    {
        _status = status;
    }

    /// <summary>
    /// Get the current version of the application.
    /// </summary>
    /// <returns>An object containing details about the current version</returns>
    /// <response code="200">The application version</response>
    [HttpGet("version")]
    [ProducesResponseType(typeof(AppVersionDTO), StatusCodes.Status200OK)]
    public IActionResult GetVersion() => Ok(_status.GetAppVersionDTO());
}