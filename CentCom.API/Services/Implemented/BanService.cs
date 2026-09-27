using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.API.Models;
using CentCom.Common;
using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Common.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace CentCom.API.Services.Implemented;

public class BanService : IBanService
{
    private readonly DatabaseContext _dbContext;

    public BanService(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BanData> GetBanAsync(int ban)
    {
        var result = await _dbContext.Bans
            .AsNoTracking()
            .Include(x => x.JobBans)
            .Include(x => x.SourceNavigation)
            .FirstOrDefaultAsync(x => x.Id == ban);
        return result == null ? null : BanData.FromBan(result);
    }

    public async Task<IEnumerable<BanData>> GetBansForKeyAsync(string key, int? source, bool onlyActive = false)
    {
        var ckey = KeyUtilities.GetCanonicalKey(key);
        var query = _dbContext.Bans
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
        var query = _dbContext.Bans
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

    public async Task<BanSearchPage> SearchSummariesForKeyAsync(string key, int page)
    {
        const int pageSize = 25;
        if (page < 1 || page > 1000)
            throw new ArgumentOutOfRangeException(nameof(page), "Search page must be between 1 and 1000.");

        key = KeyUtilities.GetCanonicalKey(key);
        var query = BanSearchQuery.ForCKeySubstring(_dbContext, key)
            .GroupBy(x => x.CKey,
            (k, g) => new KeySummary
            {
                CKey = k,
                ServerBans = g.Sum(y => y.BanType == BanType.Server ? 1 : 0),
                JobBans = g.Sum(y => y.BanType == BanType.Job ? 1 : 0),
                LatestBan = g.Max(x => x.BannedOn)
            });

        var results = await query.OrderByDescending(x => x.LatestBan)
            .ThenBy(x => x.CKey)
            .Skip((page - 1) * pageSize)
            .Take(pageSize + 1)
            .ToListAsync();
        var hasNextPage = results.Count > pageSize;
        if (hasNextPage)
            results.RemoveAt(pageSize);
        return new BanSearchPage(results, page, hasNextPage);
    }
}