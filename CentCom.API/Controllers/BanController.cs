using System.Collections.Generic;
using System.Threading.Tasks;
using CentCom.API.Models;
using CentCom.API.Services;
using CentCom.Common.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CentCom.API.Controllers;

[Produces("application/json")]
[ApiController]
public class BanController : ControllerBase
{
    private readonly IBanService _banService;
    private readonly IBanSourceService _banSourceService;

    public BanController(IBanService banService, IBanSourceService banSourceService)
    {
        _banService = banService;
        _banSourceService = banSourceService;
    }

    /// <summary>
    /// Retrieves stored bans for a provided ckey.
    /// </summary>
    /// <param name="key">A BYOND key, will be converted into CKey</param>
    /// <param name="onlyActive">Operator for controlling if only active bans will be returned</param>
    /// <param name="source">Operator for specifying a specific source to return bans for</param>
    /// <returns>A collection of bans matching the provided conditions</returns>
    /// <response code="200">The user's bans</response>
    /// <response code="400">Key was null or whitespace</response>
    [HttpGet("ban/search/{key}")]
    [ProducesResponseType(typeof(IEnumerable<BanData>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBansForKey(string key, [FromQuery] bool onlyActive, [FromQuery] int? source)
    {
        if (key == null || string.IsNullOrWhiteSpace(key))
        {
            return BadRequest("Key cannot be empty or null");
        }
        return Ok(await _banService.GetBansForKeyAsync(key, source, onlyActive));
    }

    /// <summary>
    /// Lists all available ban sources
    /// </summary>
    /// <returns>A collection of ban sources</returns>
    /// <response code="200">The list of ban sources</response>
    [HttpGet("source/list")]
    [ProducesResponseType(typeof(IEnumerable<BanSourceData>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSources()
    {
        return Ok(await _banSourceService.GetAllBanSourcesAsync());
    }

    /// <summary>
    /// Retrieves a specific ban from CentCom using the internal ID
    /// </summary>
    /// <param name="id">The CentCom Ban ID of the ban</param>
    /// <returns>The ban specified</returns>
    /// <response code="200">The desired ban</response>
    /// <response code="404">Ban ID was invalid</response>
    [HttpGet("ban/{id}")]
    [ProducesResponseType(typeof(BanData), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBan(int id)
    {
        var result = await _banService.GetBanAsync(id);
        if (result == null)
        {
            return NotFound("Invalid Ban ID");
        }
        return Ok(result);
    }
}