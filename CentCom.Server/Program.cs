using System;
using System.Net.Http;
using System.Threading.Tasks;
using CentCom.Common.Configuration;
using CentCom.Common.Util;
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
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Logger(lc =>
            {
                lc.Filter.ByExcluding(Matching.FromSource("Quartz"));
                lc.WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj} {Properties:j}{NewLine}{Exception}");
            })
            .WriteTo.Logger(lc =>
            {
                lc.WriteTo.File(path: "centcom-parser-server.txt",
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 10,
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj} {Properties:j}{NewLine}{Exception}");
            })
            .CreateLogger();

        try
        {
            var commit = AssemblyInformation.Current.Commit;
            Log.Logger.ForContext<Program>()
                .Information("Starting CentCom Server {Version} ({Commit})", AssemblyInformation.Current.Version,
                    commit?[..Math.Min(7, commit.Length)]);

            var builder = Host.CreateApplicationBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddUserSecrets<Program>(optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);
            builder.Services.AddSerilog();
            builder.Services.AddCentComDatabase(builder.Configuration);

            builder.Services.AddHttpClient<BeeBanService>();
            builder.Services.AddSingleton<VgBanService>();
            builder.Services.AddHttpClient<YogBanService>();
            builder.Services.AddHttpClient<TGMCBanService>();
            builder.Services.AddHttpClient<TgBanService>();
            builder.Services.AddHttpClient<StandardProviderService>();

            var fulpClient = builder.Services.AddHttpClient<FulpBanService>();
            if (builder.Configuration.GetSection("sourceConfig").GetValue<bool>("allowFulpExpiredSSL"))
                fulpClient.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                });

            foreach (var parser in BanParserTypes.All)
                builder.Services.AddTransient(parser);

            builder.Services.AddTransient<FlatDataImporter>();
            builder.Services.AddTransient<DatabaseUpdater>();

            builder.Services.AddQuartz(q =>
            {
                q.ScheduleJob<DatabaseUpdater>(trigger =>
                        trigger
                            .StartNow()
                            .WithIdentity("updater"),
                    job => job.WithIdentity("updater"));
            });
            builder.Services.AddQuartzHostedService(o => { o.WaitForJobsToComplete = true; });

            using var host = builder.Build();
            await host.RunAsync();
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
