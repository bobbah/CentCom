using System.Collections.Generic;
using System.Threading.Tasks;
using CentCom.API.Services;
using CentCom.API.V1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CentCom.API.V1.Controllers;

[ApiController]
[Route("api/v{apiVersion:apiVersion}/[controller]")]
[Produces("application/json")]
public class SourceController : ControllerBase
{
    private readonly IBanSourceService _banSourceService;

    public SourceController(IBanSourceService banSourceService)
    {
        _banSourceService = banSourceService;
    }

    /// <summary>
    /// Lists all available ban sources
    /// </summary>
    /// <returns>A collection of ban sources</returns>
    /// <response code="200">The list of ban sources</response>
    [HttpGet("list")]
    [ProducesResponseType(typeof(IEnumerable<BanSourceData>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSources()
    {
        return Ok(await _banSourceService.GetAllBanSourcesAsync());
    }
}