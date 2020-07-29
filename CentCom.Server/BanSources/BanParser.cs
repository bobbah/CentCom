using CentCom.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;

namespace CentCom.Server.BanSources
{
    [DisallowConcurrentExecution]
    public abstract class BanParser : IJob
    {
        protected ILogger _logger;
        protected DatabaseContext _dbContext { get; set; }
        public virtual Dictionary<string, BanSource> Sources { get; }
        public virtual bool SourceSupportsBanIDs { get; }

        public BanParser(DatabaseContext dbContext, ILogger<BanParser> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public virtual async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Firing ban parsing for {GetType().Name}");
            var storedBans = await _dbContext.Bans
                .Where(x => Sources.Keys.Contains(x.SourceNavigation.Name))
                .Include(x => x.JobBans)
                .Include(x => x.SourceNavigation)
                .ToListAsync();

            var isCompleteRefresh = context.JobDetail.JobDataMap.GetBoolean("completeRefresh") || storedBans.Count == 0;
            IEnumerable<Ban> bans = await (isCompleteRefresh ? FetchAllBansAsync() : FetchNewBansAsync());

            // Assign proper sources
            bans = await AssignBanSources(bans);

            // Check for ban updates
            foreach (var b in bans)
            {
                // Enssure the CKey is actually canonical
                b.MakeKeyCanonical();

                // Attempt to find matching bans in the database
                Ban matchedBan = null;
                if (SourceSupportsBanIDs)
                {
                    matchedBan = storedBans.FirstOrDefault(x => x.BanID == b.BanID);
                }
                else
                {
                    var bJobs = b.JobBans?.Select(x => x.Job).ToHashSet();
                    matchedBan = storedBans.FirstOrDefault(x =>
                        x.SourceNavigation.Name == b.SourceNavigation.Name
                        && x.BannedOn == b.BannedOn
                        && x.BanType == b.BanType
                        && x.CKey == b.CKey
                        && x.BannedBy == b.BannedBy
                        && ((x.JobBans == null && b.JobBans == null) 
                            || x.JobBans.Select(y => y.Job).ToHashSet().SetEquals(bJobs)));
                }

                // Update ban if an existing one is found
                if (matchedBan != null)
                {
                    if (matchedBan.Reason != b.Reason || matchedBan.Expires != b.Expires || matchedBan.UnbannedBy != b.UnbannedBy)
                    {
                        matchedBan.Reason = b.Reason;
                        matchedBan.Expires = b.Expires;
                        matchedBan.UnbannedBy = b.UnbannedBy;
                    }
                }
                // Otherwise add insert a new ban
                else
                {
                    _dbContext.Bans.Add(b);
                }
            }

            // Insert new changes
            _logger.LogInformation("Inserting new bans, updating modified bans...");
            await _dbContext.SaveChangesAsync();

            // Delete any missing bans if we're doing a complete refresh
            if (isCompleteRefresh)
            {
                var missingBans = new List<Ban>();
                var bansHashed = new HashSet<int>(bans.Select(x => x.Id));
                foreach (var b in storedBans)
                {
                    if (!bansHashed.Contains(b.Id))
                    {
                        missingBans.Add(b);
                    }
                }
                _dbContext.RemoveRange(missingBans);

                // Apply deletions
                _logger.LogInformation("Removing deleted bans...");
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<string, BanSource>> GetSourcesAsync()
        {
            if (Sources == null)
            {
                throw new NullReferenceException($"Sources for {GetType()} are null.");
            }

            var foundSources = await _dbContext.BanSources.Where(x => Sources.Keys.Contains(x.Name)).ToListAsync();
            if (foundSources.Count() != Sources.Count)
            {
                var missing = Sources.Keys.Except(foundSources.Select(x => x.Name)).ToList();
                foreach (var source in missing)
                {
                    _dbContext.BanSources.Add(Sources[source]);
                }
                await _dbContext.SaveChangesAsync();
                foundSources = await _dbContext.BanSources.Where(x => Sources.Keys.Contains(x.Name)).ToListAsync();
            }

            return foundSources.ToDictionary(x => x.Name);
        }

        public async Task<IEnumerable<Ban>> AssignBanSources(IEnumerable<Ban> bans)
        {
            var sources = await GetSourcesAsync();
            foreach (var b in bans)
            {
                b.SourceNavigation = sources[b.SourceNavigation.Name];
                b.Source = b.SourceNavigation.Id;
            }
            return bans;
        }

        public abstract Task<IEnumerable<Ban>> FetchNewBansAsync();
        public abstract Task<IEnumerable<Ban>> FetchAllBansAsync();
    }
}
