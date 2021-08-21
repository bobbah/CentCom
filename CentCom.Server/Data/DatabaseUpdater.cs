using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Data;
using CentCom.Server.BanSources;
using CentCom.Server.FlatData;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CentCom.Server.Data
{
    [DisallowConcurrentExecution]
    public class DatabaseUpdater : IJob
    {
        private readonly DatabaseContext _dbContext;
        private readonly FlatDataImporter _importer;
        private readonly ILogger _logger;
        private readonly ISchedulerFactory _schedulerFactory;

        public DatabaseUpdater(DatabaseContext dbContext, ILogger<DatabaseUpdater> logger, FlatDataImporter importer,
            ISchedulerFactory schedulerFactory)
        {
            _dbContext = dbContext;
            _logger = logger;
            _importer = importer;
            _schedulerFactory = schedulerFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Checking for any pending migrations");
            var appliedMigration = await _dbContext.Migrate(context.CancellationToken);
            if (appliedMigration)
                _logger.LogInformation("Applied new migration");

            // Import any new flat data prior to registering ban parsing jobs
            _logger.LogInformation("Checking for any updates to flat file data");
            await _importer.RunImports();

            // Call register jobs after db migration to ensure that the DB is actually created on first run before doing any ops
            _logger.LogInformation("Registering ban parsing jobs");
            await RegisterJobs();
        }

        private async Task RegisterJobs()
        {
            var parsers = AppDomain.CurrentDomain.GetAssemblies().Aggregate(new List<Type>(), (curr, next) =>
            {
                curr.AddRange(next.GetTypes().Where(x => x.IsSubclassOf(typeof(BanParser))));
                return curr;
            });

            // Get a scheduler instance
            var scheduler = await _schedulerFactory.GetScheduler();

            foreach (var p in parsers)
            {
                var regularJob = JobBuilder.Create(p)
                    .WithIdentity(p.Name, "parsers")
                    .Build();

                // var regularTrigger = TriggerBuilder.Create()
                //     .WithIdentity($"{p.Name}Trigger", "parsers")
                //     .UsingJobData("completeRefresh", false)
                //     .WithCronSchedule("0 5-25/5,35-55/5 * * * ?") // Every 5 minutes except at the half hours
                //     .StartNow()
                //     .Build();
                //
                // var fullTrigger = TriggerBuilder.Create()
                //     .WithIdentity($"{p.Name}FullRefreshTrigger", "parsersFullRefresh")
                //     .UsingJobData("completeRefresh", true)
                //     .WithCronSchedule("0 0,30 * * * ?") // Every half hour
                //     .StartNow()
                //     .Build();

                var regularTrigger = TriggerBuilder.Create()
                    .WithIdentity($"{p.Name}Trigger", "parsers")
                    .UsingJobData("completeRefresh", false)
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(5))
                    .StartNow()
                    .Build();

                var fullTrigger = TriggerBuilder.Create()
                    .WithIdentity($"{p.Name}FullRefreshTrigger", "parsersFullRefresh")
                    .UsingJobData("completeRefresh", true)
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(10))
                    .StartAt(DateTimeOffset.UtcNow.AddMinutes(2))
                    .Build();

                await scheduler.ScheduleJob(regularJob, new[] { regularTrigger, fullTrigger }, false);
            }
        }
    }
}