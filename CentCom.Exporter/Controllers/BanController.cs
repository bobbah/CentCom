using System;
using System.Threading.Tasks;
using CentCom.Exporter.Configuration;
using CentCom.Exporter.Data.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CentCom.Exporter.Controllers;

[ApiController]
[Route("api/ban")]
public class BanController : ControllerBase
{
    private readonly IBanProvider _provider;
    private readonly BanProviderOptions _providerOptions;

    public BanController(IOptions<BanProviderOptions> providerOptions, IBanProvider provider)
    {
        _providerOptions = providerOptions?.Value ??
                           throw new Exception("Invalid or missing ban provider configuration");
        _provider = provider;
    }

    [HttpGet]
    public async Task<IActionResult> GetBans(int? cursor)
    {
        return Ok(await _provider.GetBansAsync(cursor, _providerOptions));
    }
}