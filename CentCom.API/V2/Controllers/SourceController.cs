using System.Collections.Generic;
using System.Threading.Tasks;
using CentCom.API.Services;
using CentCom.API.V1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CentCom.API.V2.Controllers;

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
    /// <response code="200">The list of ban sources</response>
    [HttpGet("list")]
    [ProducesResponseType(typeof(IEnumerable<BanSourceData>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSources()
    {
        return Ok(await _banSourceService.GetAllBanSourcesAsync());
    }
    
    /// <summary>
    /// Gets details for a specified ban source
    /// </summary>
    /// <param name="id">The internal CentCom ID of the ban source</param>
    /// <response code="200">The details of the ban source</response>
    /// <response code="404">The ban source ID was invalid or missing</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BanSourceData), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSourceDetails(int id)
    {
        var foundSource = await _banSourceService.GetBanSourceAsync(id);
        return foundSource != null ? Ok(await _banSourceService.GetBanSourceAsync(id)) : Problem("Ban source ID was invalid or missing", statusCode: StatusCodes.Status404NotFound);
    }
}