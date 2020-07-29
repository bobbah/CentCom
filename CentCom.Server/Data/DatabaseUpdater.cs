using CentCom.Common.Data;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace CentCom.Server.Data
{
    [DisallowConcurrentExecution]
    public class DatabaseUpdater : IJob
    {
        private DatabaseContext _dbContext { get; set; }
        private ILogger _logger;

        public DatabaseUpdater(DatabaseContext dbContext, IScheduler scheduler, ILogger<DatabaseUpdater> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Checking for any pending migrations");
            await _dbContext.Migrate(context.CancellationToken);

            // Call register jobs after db migration to ensure that the DB is actually created on first run before doing any ops
            _logger.LogInformation("Registering ban parsing jobs");
            await Program.RegisterJobs();
        }
    }
}
