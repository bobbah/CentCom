using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.BanSources;

public class TgBanParser(DatabaseContext dbContext, TgBanService banService, ILogger<TgBanParser> logger)
    : BanParser(dbContext, logger)
{
    protected override Dictionary<string, BanSource> Sources => new()
    {
        { "tgstation", new BanSource
        {
            Display = "/tg/station",
            Name = "tgstation",
            RoleplayLevel = RoleplayLevel.Low
        } }
    };

    protected override bool SourceSupportsBanIDs => true;
    protected override string Name => "/tg/station";

    public override async Task<List<Ban>> FetchAllBansAsync()
    {
        Logger.LogInformation("Fetching all bans for /tg/station...");
        return await banService.GetBansBatchedAsync(Sources["tgstation"]);
    }

    public override async Task<List<Ban>> FetchNewBansAsync()
    {
        Logger.LogInformation("Fetching new bans for /tg/station...");
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
            var bans = await banService.GetBansAsync(page);
            if (bans.Count == 0)
                break;
            
            foundBans.AddRange(bans.Select(x => x.AsBan(Sources["tgstation"])));
            
            // Check for existing bans
            if (foundBans.Any(x => recent.Any(y => y.BanID == x.BanID)))
                break;
            
            page++;
        }

        return foundBans.DistinctBy(x => x.BanID).ToList();
    }
}