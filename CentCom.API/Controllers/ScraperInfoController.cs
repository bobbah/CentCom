using Microsoft.AspNetCore.Mvc;

namespace CentCom.API.Controllers
{
    [Route("scraper")]
    public class ScraperInfoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}