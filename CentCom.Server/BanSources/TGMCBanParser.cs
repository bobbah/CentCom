using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.BanSources;

public class TGMCBanParser(DatabaseContext dbContext, TGMCBanService banService, ILogger<TGMCBanParser> logger)
    : BanParser(dbContext, logger)
{
    private const int PagesPerBatch = 3;

    protected override Dictionary<string, BanSource> Sources => new()
    {
        { "tgmc", new BanSource
        {
            Display = "TGMC",
            Name = "tgmc",
            RoleplayLevel = RoleplayLevel.Medium
        } }
    };

    protected override bool SourceSupportsBanIDs => true;
    protected override string Name => "TGMC";

    public override async Task<List<Ban>> FetchAllBansAsync()
    {
        Logger.LogInformation("Getting all bans for TGMC...");
        return await banService.GetBansBatchedAsync();
    }

    public override async Task<List<Ban>> FetchNewBansAsync()
    {
        Logger.LogInformation("Getting new bans for TGMC...");
        var recent = await DbContext.Bans
            .Where(x => Sources.Keys.Contains(x.SourceNavigation.Name))
            .OrderByDescending(x => x.BannedOn)
            .Take(5)
            .Include(x => x.JobBans)
            .Include(x => x.SourceNavigation)
            .ToListAsync();
        var foundBans = new List<Ban>();
        var page = 1;

        while (true)
        {
            var batch = await banService.GetBansBatchedAsync(page, PagesPerBatch);
            foundBans.AddRange(batch);
            if (batch.Count == 0 || batch.Any(x => recent.Any(y => y.BannedOn == x.BannedOn && y.CKey == x.CKey)))
            {
                break;
            }
            page += PagesPerBatch;
        }

        return foundBans;
    }
}