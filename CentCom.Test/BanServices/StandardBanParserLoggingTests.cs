using System.Globalization;
using System.Reflection;
using CentCom.Server.BanSources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Display;

namespace CentCom.Test.BanServices;

public class StandardBanParserLoggingTests
{
    [Test]
    public async Task MisconfiguredStandardJob_LogsItsSource()
    {
        var sink = new CapturingSink();
        using var serilog = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Sink(sink).CreateLogger();
        using var loggerFactory = new SerilogLoggerFactory(serilog);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["standardSources:0:Id"] = "another-source",
            ["standardSources:0:Display"] = "Another source"
        }).Build();
        var parser = new StandardBanParser(null!, loggerFactory.CreateLogger<StandardBanParser>(), null!, config);

        var context = DispatchProxy.Create<IJobExecutionContext, JobContextProxy>();
        var proxy = (JobContextProxy)context;
        proxy.Job = JobBuilder.Create<StandardBanParser>().WithIdentity("tgstation", "standard-parsers").Build();
        proxy.Data = new JobDataMap { ["sourceId"] = "tgstation" };

        await Assert.That(() => parser.Execute(context)).Throws<JobExecutionException>();

        var error = sink.Events.Single(e =>
            e.Exception?.Message.Contains("Could not find configuration for source tgstation") == true);
        await Assert.That(error.Exception?.Message).Contains("Could not find configuration for source tgstation");
        await Assert.That(error.Properties["SourceId"].ToString()).IsEqualTo("\"tgstation\"");
        await Assert.That(error.Properties["ParserJob"].ToString()).IsEqualTo("\"standard-parsers.tgstation\"");

        var writer = new StringWriter();
        new MessageTemplateTextFormatter("{Message:lj} {Properties:j}{NewLine}{Exception}",
                CultureInfo.InvariantCulture)
            .Format(error, writer);
        await Assert.That(writer.ToString()).Contains("tgstation");
    }

    private sealed class CapturingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }

    public class JobContextProxy : DispatchProxy
    {
        public IJobDetail Job { get; set; } = null!;
        public JobDataMap Data { get; set; } = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod?.Name switch
        {
            "get_JobDetail" => Job,
            "get_MergedJobDataMap" => Data,
            _ => throw new NotSupportedException(targetMethod?.Name)
        };
    }
}