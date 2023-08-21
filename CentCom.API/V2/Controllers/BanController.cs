using System.Collections.Generic;
using System.Threading.Tasks;
using CentCom.API.Services;
using CentCom.Common.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CentCom.API.V2.Controllers;

[ApiController]
[Route("api/v{apiVersion:apiVersion}/[controller]")]
[Produces("application/json")]
public class BanController : ControllerBase
{
    private readonly IBanService _banService;

    public BanController(IBanService banService)
    {
        _banService = banService;
    }

    /// <summary>
    /// Retrieves stored bans for a provided ckey.
    /// </summary>
    /// <param name="key">A BYOND key, will be converted into CKey</param>
    /// <param name="onlyActive">Operator for controlling if only active bans will be returned</param>
    /// <param name="source">Operator for specifying a specific source to return bans for</param>
    /// <response code="200">A collection of bans matching the provided conditions</response>
    /// <response code="400">Key was null or whitespace</response>
    [HttpGet("search/{key}")]
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
    /// Retrieves a specific ban from CentCom using the internal ID
    /// </summary>
    /// <param name="id">The CentCom Ban ID of the ban</param>
    /// <response code="200">The specified ban</response>
    /// <response code="404">Ban ID was invalid</response>
    [HttpGet("{id:int}")]
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