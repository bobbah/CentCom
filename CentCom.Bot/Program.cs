using System;
using System.Threading.Tasks;
using CentCom.Bot.Commands;
using CentCom.Bot.Configuration;
using CentCom.Bot.Jobs;
using CentCom.Bot.Responders;
using CentCom.Common.Configuration;
using CentCom.Common.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;
using Serilog;

namespace CentCom.Bot
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            // Setup Serilog
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Logger(lc =>
                {
                    lc.Filter.ByExcluding(
                        "Contains(SourceContext, 'Quartz') and (@Level = 'Information')");
                    lc.WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
                })
                .WriteTo.Logger(lc =>
                {
                    lc.WriteTo.File(path: "centcom-discord-bot.txt",
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
                })
                .CreateLogger();

            return CreateHostBuilder(args).RunConsoleAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .AddDiscordService(services =>
            {
                var configuration = services.GetRequiredService<IConfiguration>();
                return configuration.GetValue<string>("discord:token") ??
                       throw new InvalidOperationException
                       (
                           "Failed to read Discord configuration, bot token not found in appsettings.json."
                       );
            })
            .ConfigureServices((_, services) =>
            {
                // Add configuration
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddCommandLine(args)
                    .AddUserSecrets<Program>()
                    .Build();
                services.AddSingleton<IConfiguration>(config);

                // Add Discord config
                services.AddOptions<DiscordConfiguration>()
                    .Bind(config.GetSection("discord"))
                    .Validate(x => x.Token != null);

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

                // Add Quartz
                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();

                    q.ScheduleJob<FailedParseJob>(trigger =>
                            trigger
                                .StartNow()
                                .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()),
                        job => job.WithIdentity("failed-parse"));
                });
                services.AddQuartzHostedService();

                // Add Quartz jobs
                services.AddTransient<FailedParseJob>();

                // Add Discord commands
                services
                    .AddDiscordCommands(true)
                    .AddCommandTree()
                    .WithCommandGroup<AboutCommands>()
                    .WithCommandGroup<SearchCommands>()
                    .Finish()
                    .AddResponder<ServerJoinResponder>();
            })
            .UseSerilog();
    }
}