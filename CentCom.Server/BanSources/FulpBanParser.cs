using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.BanSources;

public class FulpBanParser : BanParser
{
    private const int PagesPerBatch = 12;
    private readonly FulpBanService _banService;

    public FulpBanParser(DatabaseContext dbContext, FulpBanService banService, ILogger<FulpBanParser> logger) : base(dbContext, logger)
    {
        _banService = banService;
        Logger = logger;
    }

    protected override Dictionary<string, BanSource> Sources => new()
    {
        { "fulp", new BanSource
        {
            Display = "Fulpstation",
            Name = "fulp",
            RoleplayLevel = RoleplayLevel.Medium
        } }
    };

    protected override bool SourceSupportsBanIDs => false;
    protected override string Name => "Fulpstation";

    public override async Task<List<Ban>> FetchAllBansAsync()
    {
        Logger.LogInformation("Getting all bans for Fulpstation...");
        return await _banService.GetBansBatchedAsync();
    }

    public override async Task<List<Ban>> FetchNewBansAsync()
    {
        Logger.LogInformation("Getting new bans for Fulpstation...");
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
            var batch = await _banService.GetBansBatchedAsync(page, PagesPerBatch);
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