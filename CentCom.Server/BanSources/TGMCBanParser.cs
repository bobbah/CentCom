using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.BanSources;

public class TGMCBanParser : BanParser
{
    private const int PAGES_PER_BATCH = 3;
    private readonly TGMCBanService _banService;

    public TGMCBanParser(DatabaseContext dbContext, TGMCBanService banService, ILogger<TGMCBanParser> logger) : base(dbContext, logger)
    {
        _banService = banService;
    }

    protected override Dictionary<string, BanSource> Sources => new Dictionary<string, BanSource>
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

    public override async Task<IEnumerable<Ban>> FetchAllBansAsync()
    {
        Logger.LogInformation("Getting all bans for TGMC...");
        return await _banService.GetBansBatchedAsync();
    }

    public override async Task<IEnumerable<Ban>> FetchNewBansAsync()
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
            var batch = await _banService.GetBansBatchedAsync(page, PAGES_PER_BATCH);
            foundBans.AddRange(batch);
            if (!batch.Any() || batch.Any(x => recent.Any(y => y.BannedOn == x.BannedOn && y.CKey == y.CKey)))
            {
                break;
            }
            page += PAGES_PER_BATCH;
        }

        return foundBans;
    }
}