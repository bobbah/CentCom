using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentCom.Server.BanSources
{
    public class TgBanParser : BanParser
    {
        public override Dictionary<string, BanSource> Sources => new Dictionary<string, BanSource>()
        {
            { "tgstation", new BanSource()
            {
                Display = "/tg/station",
                Name = "tgstation",
                RoleplayLevel = RoleplayLevel.Low
            } }
        };
        public override bool SourceSupportsBanIDs => true;
        private readonly TgBanService _banService;

        public TgBanParser(DatabaseContext dbContext, TgBanService banService, ILogger<TgBanParser> logger) : base(dbContext, logger)
        {
            _banService = banService;
        }

        public override async Task<IEnumerable<Ban>> FetchAllBansAsync()
        {
            _logger.LogInformation("Fetching all bans for /tg/station...");
            return await _banService.GetBansBatchedAsync();
        }

        public override async Task<IEnumerable<Ban>> FetchNewBansAsync()
        {
            _logger.LogInformation("Fetching new bans for /tg/station...");
            var recent = await _dbContext.Bans
                .Where(x => Sources.Keys.Contains(x.SourceNavigation.Name))
                .OrderByDescending(x => x.BannedOn)
                .Take(5)
                .Include(x => x.JobBans)
                .Include(x => x.SourceNavigation)
                .ToListAsync();
            return await _banService.GetBansBatchedAsync(searchFor: recent.Select(x => x.Id));
        }
    }
}
