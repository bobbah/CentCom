using System.Collections.Generic;
using System.Threading.Tasks;
using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.BanSources;

public class VgBanParser(DatabaseContext dbContext, VgBanService banService, ILogger<VgBanParser> logger)
    : BanParser(dbContext, logger)
{
    protected override Dictionary<string, BanSource> Sources => new()
    {
        { "vgstation", new BanSource
        {
            Display = "/vg/station",
            Name = "vgstation",
            RoleplayLevel = RoleplayLevel.Low
        } }
    };

    protected override bool SourceSupportsBanIDs => false;
    protected override string Name => "/vg/station";

    public override async Task<List<Ban>> FetchAllBansAsync()
    {
        Logger.LogInformation("Fetching all bans for /vg/station...");
        return await banService.GetBansAsync();
    }

    public override async Task<List<Ban>> FetchNewBansAsync()
    {
        // Note that the /vg/station website only has a single page for bans, so we always do a full refresh
        Logger.LogInformation("Fetching new bans for /vg/station...");
        return await banService.GetBansAsync();
    }
}