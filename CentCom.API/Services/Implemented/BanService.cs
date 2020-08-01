using CentCom.API.Models;
using CentCom.Common.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CentCom.API.Services.Implemented
{
    public class BanService : IBanService
    {
        private DatabaseContext _dbContext;
        private static Regex _canonicalKeyInvalidChars = new Regex(@"[^a-z0-9]");

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
            var ckey = _canonicalKeyInvalidChars.Replace(key.ToLower(), "");
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
                query = query.Where(x => x.UnbannedBy != null && (x.Expires == null || x.Expires > DateTime.UtcNow));
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
                query = query.Where(x => x.UnbannedBy != null && (x.Expires == null || x.Expires > DateTime.UtcNow));
            }
            return await query.OrderByDescending(x => x.BannedOn)
                .Select(x => BanData.FromBan(x))
                .ToListAsync();
        }
    }
}
