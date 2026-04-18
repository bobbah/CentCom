using CentCom.API.Models;
using CentCom.Common;
using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Common.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace CentCom.API.Services.Implemented;

public class BanService(IDbContextFactory<NpgsqlDbContext> dbContextFactory) : IBanService
{
    public async Task<BanData> GetBanAsync(int ban)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return BanData.FromBan(await dbContext.Bans
            .Include(x => x.JobBans)
            .Include(x => x.SourceNavigation)
            .FirstOrDefaultAsync(x => x.Id == ban));
    }

    public async Task<IEnumerable<BanData>> GetBansForKeyAsync(string key, int? source, bool onlyActive = false)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var ckey = KeyUtilities.GetCanonicalKey(key);
        var query = dbContext.Bans
            .Include(x => x.JobBans)
            .Include(x => x.SourceNavigation)
            .Where(x => x.CKey == ckey);

        if (source.HasValue)
        {
            query = query.Where(x => x.Source == source);
        }

        if (onlyActive)
        {
            query = query.Where(x => x.UnbannedBy == null && (x.Expires == null || x.Expires > DateTime.UtcNow));
        }

        return await query.OrderByDescending(x => x.BannedOn)
            .Select(x => BanData.FromBan(x))
            .ToListAsync();
    }

    public async Task<IEnumerable<BanData>> GetBansForSourceAsync(int source, bool onlyActive = false)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var query = dbContext.Bans
            .Include(x => x.JobBans)
            .Include(x => x.SourceNavigation)
            .Where(x => x.Source == source);

        if (onlyActive)
        {
            query = query.Where(x => x.UnbannedBy == null && (x.Expires == null || x.Expires > DateTime.UtcNow));
        }

        return await query.OrderByDescending(x => x.BannedOn)
            .Select(x => BanData.FromBan(x))
            .ToListAsync();
    }

    public async Task<IEnumerable<KeySummary>> SearchSummariesForKeyAsync(string key)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        key = KeyUtilities.GetCanonicalKey(key);
        var query = dbContext.Bans.GroupBy(x => x.CKey,
            (k, g) => new KeySummary
            {
                CKey = k,
                ServerBans = g.Sum(y => y.BanType == BanType.Server ? 1 : 0),
                JobBans = g.Sum(y => y.BanType == BanType.Job ? 1 : 0),
                LatestBan = g.Max(x => x.BannedOn)
            }).Where(x => x.CKey.ToLower().Contains(key));

        return await query.OrderByDescending(x => x.LatestBan)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> SearchCkeys(string key, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        key = KeyUtilities.GetCanonicalKey(key);
        var query = dbContext.Bans.GroupBy(x => x.CKey).Where(x => x.Key.ToLower().Contains(key));
        return await query.Select(x => x.Key).Take(64).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BanSourceTotalData>> GetBanTotalsBySourceAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Bans
            .GroupBy(
                x => new { x.Source, x.SourceNavigation.Display },
                (source, bans) => new BanSourceTotalData
                {
                    SourceID = source.Source,
                    SourceName = source.Display,
                    TotalBans = bans.Count()
                })
            .OrderByDescending(x => x.TotalBans)
            .ThenBy(x => x.SourceName)
            .ToListAsync();
    }

    public async Task<IEnumerable<BanData>> GetLatestBansAsync(int count)
    {
        var clampedCount = Math.Clamp(count, 1, 100);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Bans
            .Include(x => x.JobBans)
            .Include(x => x.SourceNavigation)
            .OrderByDescending(x => x.BannedOn)
            .Take(clampedCount)
            .Select(x => BanData.FromBan(x))
            .ToListAsync();
    }
}
