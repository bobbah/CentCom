using Microsoft.AspNetCore.Mvc;

namespace CentCom.API.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("scraper")]
public class ScraperInfoController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}