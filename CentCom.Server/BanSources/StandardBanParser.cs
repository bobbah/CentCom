using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;
using CentCom.Common.Models;
using CentCom.Server.Configuration;
using CentCom.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CentCom.Server.BanSources;

public class StandardBanParser : BanParser
{
    private readonly StandardProviderService _banService;
    private readonly List<StandardProviderConfiguration> _providerConfigs;
    private string _name;

    private Dictionary<string, BanSource> _sources;

    public StandardBanParser(DatabaseContext dbContext, ILogger<StandardBanParser> logger,
        StandardProviderService banService,
        IConfiguration config) :
        base(dbContext, logger)
    {
        _banService = banService;
        _providerConfigs = config.GetSection("standardSources").Get<List<StandardProviderConfiguration>>();
    }

    protected override Dictionary<string, BanSource> Sources => _sources;

    protected override bool SourceSupportsBanIDs => true;
    protected override string Name => _name;

    protected override Task Configure(IJobExecutionContext context)
    {
        // Get the source from the job context
        var sourceId = context.MergedJobDataMap["sourceId"].ToString();
        var source = _providerConfigs.FirstOrDefault(x => x.Id == sourceId);
        if (source == null)
            throw new Exception($"Could not find configuration for source {sourceId}");

        // Configure the ban service for this source
        _name = source.Display;
        _banService.Configure(source);

        // Ensure source is set
        _sources = new Dictionary<string, BanSource>
        {
            { _banService.Source.Name, _banService.Source }
        };

        return Task.CompletedTask;
    }

    public override async Task<IEnumerable<Ban>> FetchNewBansAsync()
    {
        Logger.LogInformation("Fetching new bans for {Name}...", Name);
        var recent = await DbContext.Bans
            .Where(x => Sources.Keys.Contains(x.SourceNavigation.Name))
            .OrderByDescending(x => x.BannedOn)
            .Take(5)
            .Include(x => x.JobBans)
            .Include(x => x.SourceNavigation)
            .ToListAsync();
        return await _banService.GetBansBatchedAsync(searchFor: recent.Select(x => int.Parse(x.BanID)));
    }

    public override async Task<IEnumerable<Ban>> FetchAllBansAsync()
    {
        Logger.LogInformation("Fetching all bans for {Name}...", Name);
        return await _banService.GetBansBatchedAsync();
    }
}