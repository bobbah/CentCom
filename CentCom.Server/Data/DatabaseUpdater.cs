using CentCom.Common.Data;
using CentCom.Server.FlatData;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace CentCom.Server.Data
{
    [DisallowConcurrentExecution]
    public class DatabaseUpdater : IJob
    {
        private DatabaseContext _dbContext;
        private ILogger _logger;
        private FlatDataImporter _importer;

        public DatabaseUpdater(DatabaseContext dbContext, IScheduler scheduler, ILogger<DatabaseUpdater> logger, FlatDataImporter importer)
        {
            _dbContext = dbContext;
            _logger = logger;
            _importer = importer;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Checking for any pending migrations");
            await _dbContext.Migrate(context.CancellationToken);

            // Import any new flat data prior to registering ban parsing jobs
            await _importer.RunImports();

            // Call register jobs after db migration to ensure that the DB is actually created on first run before doing any ops
            _logger.LogInformation("Registering ban parsing jobs");
            await Program.RegisterJobs();
        }
    }
}
