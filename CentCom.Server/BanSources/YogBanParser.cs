using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.BanSources;

public class YogBanParser(DatabaseContext dbContext, YogBanService banService, ILogger<YogBanParser> logger)
    : BanParser(dbContext, logger)
{
    private const int PagesPerBatch = 12;

    protected override Dictionary<string, BanSource> Sources => new()
    {
        { "yogstation", new BanSource
        {
            Display = "YogStation",
            Name = "yogstation",
            RoleplayLevel = RoleplayLevel.Medium
        } }
    };

    protected override bool SourceSupportsBanIDs => true;
    protected override string Name => "YogStation";

    public override async Task<List<Ban>> FetchNewBansAsync()
    {
        Logger.LogInformation("Getting new bans for YogStation...");
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
            var batch = (await banService.GetBansBatchedAsync(page, PagesPerBatch)).ToArray();
            foundBans.AddRange(batch);
            if (!batch.Any() || batch.Any(x => recent.Any(y => y.BanID == x.BanID)))
            {
                break;
            }
            page += PagesPerBatch;
        }

        return foundBans;
    }

    public override async Task<List<Ban>> FetchAllBansAsync()
    {
        Logger.LogInformation("Getting all bans for YogStation...");
        return await banService.GetBansBatchedAsync();
    }
}