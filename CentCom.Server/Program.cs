using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CentCom.Common.Configuration;
using CentCom.Common.Data;
using CentCom.Server.BanSources;
using CentCom.Server.Data;
using CentCom.Server.FlatData;
using CentCom.Server.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;
using Serilog.Filters;

namespace CentCom.Server;

internal class Program
{
    static Task Main(string[] args)
    {
        // Setup Serilog
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Logger(lc =>
            {
                lc.Filter.ByExcluding(Matching.FromSource("Quartz"));
                lc.WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
            })
            .WriteTo.Logger(lc =>
            {
                lc.WriteTo.File(path: "centcom-parser-server.txt",
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
            })
            .CreateLogger();

        Log.Logger.ForContext<Program>()
            .Information("Starting CentCom Server {Version}", Assembly.GetExecutingAssembly().GetName().Version);

        return CreateHostBuilder(args).RunConsoleAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices((_, services) =>
            {
                // Add configuration
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddCommandLine(args)
                    .AddUserSecrets<Program>()
                    .Build();
                services.AddSingleton<IConfiguration>(config);

                // Add logging
                services.AddLogging(loggingBuilder =>
                    loggingBuilder.AddSerilog(dispose: true));

                // Get DB configuration
                var dbConfig = new DbConfig();
                config.Bind("dbConfig", dbConfig);

                // Add appropriate DB context
                if (dbConfig == null)
                {
                    throw new Exception(
                        "Failed to read DB configuration, please ensure you provide one in appsettings.json");
                }

                switch (dbConfig.DbType)
                {
                    case DbType.Postgres:
                        services.AddDbContext<DatabaseContext, NpgsqlDbContext>();
                        break;
                    case DbType.MariaDB:
                        services.AddDbContext<DatabaseContext, MariaDbContext>();
                        break;
                    case DbType.MySql:
                        services.AddDbContext<DatabaseContext, MySqlDbContext>();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // Add ban services as singletons
                services.AddSingleton<BeeBanService>();
                services.AddSingleton<VgBanService>();
                services.AddSingleton<YogBanService>();
                services.AddSingleton<FulpBanService>();
                services.AddSingleton<TGMCBanService>();
                services.AddSingleton<TgBanService>();
                    
                // Standard provider is transient as it differs per request
                services.AddTransient<StandardProviderService>();

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

                // Add Quartz
                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();

                    q.ScheduleJob<DatabaseUpdater>(trigger =>
                            trigger
                                .StartNow()
                                .WithIdentity("updater"),
                        job => job.WithIdentity("updater"));
                });
                services.AddQuartzHostedService();
            });
}