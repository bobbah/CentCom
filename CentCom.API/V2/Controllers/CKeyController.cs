using System.Collections.Generic;
using System.Threading.Tasks;
using CentCom.API.Services;
using CentCom.Common;
using CentCom.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CentCom.API.V2.Controllers;

[ApiController]
[Route("api/v{apiVersion:apiVersion}/[controller]")]
[Produces("application/json")]
public class CKeyController : ControllerBase
{
    private readonly IBanService _banService;

    public CKeyController(IBanService banService)
    {
        _banService = banService;
    }

    /// <summary>
    /// Search through the set of known canonical keys using a provided search query
    /// </summary>
    /// <param name="query">The search query</param>
    /// <response code="200">A list of keys matching the search query, along with statistics about them</response>
    /// <response code="400">The provided search query was missing or invalid</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<KeySummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(string query)
    {
        var cleanQuery = KeyUtilities.GetCanonicalKey(query ?? string.Empty);
        if (string.IsNullOrWhiteSpace(cleanQuery) || cleanQuery.Length < 3 || cleanQuery.Length > 32)
        {
            return Problem(
                $"Provided search term must be between 3-32 characters after being converted to CKey form. (Raw query: '{query}', CKey form: '{cleanQuery}')",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok(await _banService.SearchSummariesForKeyAsync(cleanQuery));
    }
}