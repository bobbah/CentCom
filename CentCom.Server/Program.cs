using CentCom.Common.Configuration;
using CentCom.Common.Data;
using CentCom.Server.BanSources;
using CentCom.Server.Data;
using CentCom.Server.FlatData;
using CentCom.Server.Quartz;
using CentCom.Server.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Serilog;
using Serilog.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CentCom.Server
{
    class Program
    {
        private static IScheduler _scheduler;
        private static IServiceProvider _serviceProvider;
        private static IConfiguration _configuration;

        static async Task Main(string[] args)
        {
            // Get application configuration
            BuildConfiguration(args);

            // Setup Serilog
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Logger(lc =>
                {
                    lc.Filter.ByExcluding(Matching.FromSource("Quartz"));
                    lc.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
                })
                .WriteTo.Logger(lc =>
                {
                    lc.WriteTo.File(path: "centcom-parser-server.txt",
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
                })
                .CreateLogger();

            Log.Logger.ForContext<Program>()
                .Information($"Starting CentCom Server v{Assembly.GetExecutingAssembly().GetName().Version}");

            // Get a scheduler factory and scheduler
            StdSchedulerFactory factory = new StdSchedulerFactory();
            _scheduler = await factory.GetScheduler();

            // Build services provider and register it with the job factory
            RegisterServices();
            _scheduler.JobFactory = new JobFactory(_serviceProvider);

            // Add updater job
            IJobDetail job = JobBuilder.Create<DatabaseUpdater>()
                .WithIdentity("updater")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("updaterTrigger")
                .StartNow()
                .Build();

            await _scheduler.ScheduleJob(job, trigger);

            // Start scheduler
            await _scheduler.Start();

            Log.Logger.ForContext<Program>().Information("Startup completed");

            // Run infinitely
            await Task.Delay(-1);
            DisposeServices();
        }

        public static void BuildConfiguration(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddCommandLine(args)
                .Build();
        }

        public static async Task RegisterJobs()
        {
            var parsers = AppDomain.CurrentDomain.GetAssemblies().Aggregate(new List<Type>(), (curr, next) =>
            {
                curr.AddRange(next.GetTypes().Where(x => x.IsSubclassOf(typeof(BanParser))));
                return curr;
            });

            foreach (var p in parsers)
            {
                IJobDetail regularJob = JobBuilder.Create(p)
                    .WithIdentity(p.Name, "parsers")
                    .Build();

                ITrigger regularTrigger = TriggerBuilder.Create()
                    .WithIdentity($"{p.Name}Trigger", "parsers")
                    .UsingJobData("completeRefresh", false)
                    .WithCronSchedule("0 5-25/5,35-55/5 * * * ?") // Every 5 minutes except at the half hours
                    .StartNow()
                    .Build();

                ITrigger fullTrigger = TriggerBuilder.Create()
                    .WithIdentity($"{p.Name}FullRefreshTrigger", "parsersFullRefresh")
                    .UsingJobData("completeRefresh", true)
                    .WithCronSchedule("0 0,30 * * * ?") // Every half hour
                    .StartNow()
                    .Build();

                await _scheduler.ScheduleJob(regularJob, new[] { regularTrigger, fullTrigger }, false);
            }
        }

        public static void RegisterServices()
        {
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));

            // Add scheduler
            services.AddSingleton(_scheduler);

            // Add config
            services.AddSingleton(_configuration);

            // Get DB configuration
            var dbConfig = new DbConfig();
            _configuration.Bind("dbConfig", dbConfig);

            // Add appropriate DB context
            if (dbConfig == null)
            {
                throw new Exception("Failed to read DB configuration, please ensure you provide one in appsettings.json");
            }
            switch (dbConfig.DbType)
            {
                case DbType.Postgres:
                    services.AddDbContext<DatabaseContext, NpgsqlDbContext>();
                    break;
                case DbType.MariaDB:
                case DbType.MySql:
                    services.AddDbContext<DatabaseContext, MySqlDbContext>();
                    break;
            }

            // Add ban services as singletons
            services.AddSingleton<BeeBanService>();
            services.AddSingleton<VgBanService>();
            services.AddSingleton<YogBanService>();
            services.AddSingleton<FulpBanService>();
            services.AddSingleton<TGMCBanService>();
            services.AddSingleton<TgBanService>();

            // Add ban parsers
            var parsers = AppDomain.CurrentDomain.GetAssemblies().Aggregate(new List<Type>(), (curr, next) =>
            {
                curr.AddRange(next.GetTypes().Where(x => x.IsSubclassOf(typeof(BanParser))));
                return curr;
            });

            foreach (var p in parsers)
            {
                services.AddTransient(p);
            }

            // Add jobs
            services.AddTransient<FlatDataImporter>();
            services.AddTransient<DatabaseUpdater>();

            _serviceProvider = services.BuildServiceProvider(true);
        }

        public static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }

            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
