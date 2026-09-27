using CentCom.Common;
using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Common.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace CentCom.MCP.Services.Implemented;

public class BanDataService(DatabaseContext dbContext) : IBanDataService
{
    private const int MaxSearchLimit = 50;

    public async Task<IEnumerable<BanData>> GetBansForKeyAsync(
        string key,
        int? source,
        bool onlyActive = false,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        BanType? banType = null)
    {
        var ckey = KeyUtilities.GetCanonicalKey(key);
        var query = dbContext.Bans
            .Include(x => x.JobBans)
            .Include(x => x.SourceNavigation)
            .Where(x => x.CKey == ckey);

        if (source.HasValue)
            query = query.Where(x => x.Source == source);

        if (onlyActive)
            query = query.Where(x => x.UnbannedBy == null && (x.Expires == null || x.Expires > DateTime.UtcNow));

        if (createdAfter.HasValue)
            query = query.Where(x => x.BannedOn >= createdAfter.Value);

        if (createdBefore.HasValue)
            query = query.Where(x => x.BannedOn <= createdBefore.Value);

        if (banType.HasValue)
            query = query.Where(x => x.BanType == banType.Value);

        return await query.OrderByDescending(x => x.BannedOn)
            .Select(x => BanData.FromBan(x))
            .ToListAsync();
    }

    public async Task<IEnumerable<BanSourceData>> GetAllBanSourcesAsync()
    {
        return await dbContext.BanSources
            .OrderBy(x => x.Display)
            .Select(x => new BanSourceData
            {
                ID = x.Id,
                Name = x.Display,
                RoleplayLevel = x.RoleplayLevel
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<KeySummary>> SearchUsersAsync(string query, int limit = 25)
    {
        var ckey = KeyUtilities.GetCanonicalKey(query);
        var effectiveLimit = Math.Clamp(limit, 1, MaxSearchLimit);

        return await BanSearchQuery.ForCKeySubstring(dbContext, ckey)
            .GroupBy(x => x.CKey, (k, g) => new KeySummary
            {
                CKey = k,
                ServerBans = g.Sum(y => y.BanType == BanType.Server ? 1 : 0),
                JobBans = g.Sum(y => y.BanType == BanType.Job ? 1 : 0),
                LatestBan = g.Max(x => x.BannedOn)
            })
            .OrderByDescending(x => x.LatestBan)
            .ThenBy(x => x.CKey)
            .Take(effectiveLimit)
            .ToListAsync();
    }
}