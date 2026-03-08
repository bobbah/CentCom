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
    public async Task<IActionResult> SearchBans(string key)
    {
        var ckey = KeyUtilities.GetCanonicalKey(key);
        if (string.IsNullOrWhiteSpace(ckey) || ckey.Length < 3)
        {
            return View("badsearch", new BanSearchViewModel { CKey = ckey });
        }

        var searchResults = await _banService.SearchSummariesForKeyAsync(key);

        // If there is only one result, just view it
        if (searchResults.Count() == 1)
        {
            return RedirectToAction("ViewBans", new { key = searchResults.First().CKey });
        }

        return View(new BanSearchViewModel { CKey = ckey, Data = searchResults });
    }

    [HttpGet("viewer/view/{key}")]
    public async Task<IActionResult> ViewBans(string key, bool onlyActive = false)
    {
        var bans = await _banService.GetBansForKeyAsync(key, null, onlyActive);

        return View(new BanViewViewModel { CKey = KeyUtilities.GetCanonicalKey(key), Bans = bans, OnlyActive = onlyActive });
    }
}