using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;
using CentCom.Server.BanSources;
using CentCom.Server.Configuration;
using CentCom.Server.FlatData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CentCom.Server.Data;

[DisallowConcurrentExecution]
public class DatabaseUpdater(
    DatabaseContext dbContext,
    ILogger<DatabaseUpdater> logger,
    FlatDataImporter importer,
    ISchedulerFactory schedulerFactory,
    IConfiguration config)
    : IJob
{
    /// <summary>
    /// Types of BanParsers which should not be automatically configured with a refresh schedule
    /// </summary>
    private readonly List<Type> _autoConfigBypass = new()
    {
        typeof(StandardBanParser)
    };

    private readonly ILogger _logger = logger;
    private readonly List<StandardProviderConfiguration> _providerConfigs = config.GetSection("standardSources").Get<List<StandardProviderConfiguration>>();

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Checking for any pending migrations");
        var appliedMigration = await dbContext.Migrate(context.CancellationToken);
        if (appliedMigration)
            _logger.LogInformation("Applied new migration");

        // Import any new flat data prior to registering ban parsing jobs
        _logger.LogInformation("Checking for any updates to flat file data");
        await importer.RunImports();

        // Call register jobs after db migration to ensure that the DB is actually created on first run before doing any ops
        _logger.LogInformation("Registering ban parsing jobs");
        await RegisterJobs();
        if (_providerConfigs != null)
            await RegisterStandardJobs();
    }

    /// <summary>
    /// Registers all jobs with Quartz' job scheduler for special ban sources
    /// </summary>
    private async Task RegisterJobs()
    {
        var allowedParsers = config.GetSection("enabledParsers").Get<HashSet<string>>();
        var parsers = AppDomain.CurrentDomain.GetAssemblies().Aggregate(new List<Type>(), (curr, next) =>
        {
            curr.AddRange(next.GetTypes()
                .Where(x => x.IsSubclassOf(typeof(BanParser)) && !_autoConfigBypass.Contains(x) && allowedParsers.Contains(x.Name)));
            return curr;
        });

        // Get a scheduler instance
        var scheduler = await schedulerFactory.GetScheduler();

        foreach (var p in parsers)
        {
            var regularJob = JobBuilder.Create(p)
                .WithIdentity(p.Name, "parsers")
                .Build();

            var regularTrigger = TriggerBuilder.Create()
                .WithIdentity($"{p.Name}Trigger", "parsers")
                .UsingJobData("completeRefresh", false)
                .WithCronSchedule("0 5-25/5,35-55/5 * * * ?") // Every 5 minutes except at the half hours
                .StartNow()
                .Build();
                
            var fullTrigger = TriggerBuilder.Create()
                .WithIdentity($"{p.Name}FullRefreshTrigger", "parsersFullRefresh")
                .UsingJobData("completeRefresh", true)
                .WithCronSchedule("0 0,30 * * * ?") // Every half hour
                .StartNow()
                .Build();

            await scheduler.ScheduleJob(regularJob, new[] { regularTrigger, fullTrigger }, false);
        }
    }

    /// <summary>
    /// Registers all jobs with Quartz' job scheduler for standardized ban sources
    /// </summary>
    private async Task RegisterStandardJobs()
    {
        // Get a scheduler instance
        var scheduler = await schedulerFactory.GetScheduler();

        // Iterate through each standard provider to set it up
        foreach (var provider in _providerConfigs)
        {
            var job = JobBuilder.Create<StandardBanParser>()
                .WithIdentity(provider.Id, "standard-parsers")
                .Build();

            var regularTrigger = TriggerBuilder.Create()
                .WithIdentity($"{provider.Id}Trigger", "standard-parsers")
                .UsingJobData("completeRefresh", false)
                .UsingJobData("sourceId", provider.Id)
                .WithCronSchedule("0 5-25/5,35-55/5 * * * ?") // Every 5 minutes except at the half hours
                .StartNow()
                .Build();

            var fullTrigger = TriggerBuilder.Create()
                .WithIdentity($"{provider.Id}FullRefreshTrigger", "standard-parsersFullRefresh")
                .UsingJobData("completeRefresh", true)
                .UsingJobData("sourceId", provider.Id)
                .WithCronSchedule("0 0,30 * * * ?") // Every half hour
                .StartNow()
                .Build();

            await scheduler.ScheduleJob(job, new[] { regularTrigger, fullTrigger }, false);
        }

        if (_providerConfigs?.Count > 0)
            _logger.LogInformation("Configured {Count} standardized ban providers for parsing",
                _providerConfigs?.Count);
    }
}