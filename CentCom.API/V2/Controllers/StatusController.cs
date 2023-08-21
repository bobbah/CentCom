using CentCom.API.Services;
using CentCom.API.V2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CentCom.API.V2.Controllers;

[Produces("application/json")]
[ApiController]
[Route("api/v{apiVersion:apiVersion}/[controller]")]
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
    /// <response code="200">Details of the application version</response>
    [HttpGet]
    [ProducesResponseType(typeof(AppVersionDTO), StatusCodes.Status200OK)]
    public IActionResult GetVersion() => Ok(_status.GetAppVersionDTO());
}