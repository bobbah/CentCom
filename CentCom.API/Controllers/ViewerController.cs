using System.Linq;
using System.Threading.Tasks;
using CentCom.API.Models;
using CentCom.API.Services;
using CentCom.Common;
using Microsoft.AspNetCore.Mvc;

namespace CentCom.API.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ViewerController : Controller
{
    private readonly IBanService _banService;

    public ViewerController(IBanService banService)
    {
        _banService = banService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("viewer/search/{key}")]
    public async Task<IActionResult> SearchBans(string key, [FromQuery] int page = 1)
    {
        if (page is < 1 or > 1000)
            return BadRequest("Page must be between 1 and 1000.");

        var ckey = KeyUtilities.GetCanonicalKey(key);
        if (string.IsNullOrWhiteSpace(ckey) || ckey.Length < 3)
        {
            return View("badsearch", new BanSearchViewModel { CKey = ckey });
        }

        var searchResults = await _banService.SearchSummariesForKeyAsync(key, page);

        // If there is only one result, just view it
        if (page == 1 && searchResults.Data.Count == 1 && !searchResults.HasNextPage)
        {
            return RedirectToAction("ViewBans", new { key = searchResults.Data[0].CKey });
        }

        return View(new BanSearchViewModel
        {
            CKey = ckey,
            Data = searchResults.Data,
            Page = searchResults.Page,
            HasNextPage = searchResults.HasNextPage
        });
    }

    [HttpGet("viewer/view/{key}")]
    public async Task<IActionResult> ViewBans(string key, bool onlyActive = false)
    {
        var bans = await _banService.GetBansForKeyAsync(key, null, onlyActive);

        return View(new BanViewViewModel { CKey = KeyUtilities.GetCanonicalKey(key), Bans = bans, OnlyActive = onlyActive });
    }
}