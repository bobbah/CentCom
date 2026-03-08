using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        return BanData.FromBan(await _dbContext.Bans
            .Include(x => x.JobBans)
            .Include(x => x.SourceNavigation).
            FirstOrDefaultAsync(x => x.Id == ban));
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

    public async Task<IEnumerable<KeySummary>> SearchSummariesForKeyAsync(string key)
    {
        key = KeyUtilities.GetCanonicalKey(key);
        var query = _dbContext.Bans.GroupBy(x => x.CKey,
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
}