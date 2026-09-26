using System.Net;
using CentCom.Detective;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CentCom.Test;

public class DetectiveTests
{
    private const string Ban12 =
        """[{"id":12,"banType":"Server","cKey":"alice","bannedOn":"2024-01-01T00:00:00Z","bannedBy":"admin","reason":"test","jobBans":[]}]""";

    private const string Ban11 =
        """[{"id":11,"banType":"Server","cKey":"bob","bannedOn":"2024-01-01T00:00:00Z","bannedBy":"admin","reason":"test","jobBans":[]}]""";

    [Test]
    public async Task StandardExporterChecksCursorAndParsesRealProvider()
    {
        var runner = CreateRunner(request =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(request.RequestUri!.Query.Contains("cursor=12") ? Ban11 : Ban12)
            });

        var report = await runner.RunAsync(SourceType.Standard, new Uri("https://example.org/export/"));

        await Assert
            .That(string.Join("; ", report.Results.Where(x => x.Status == "fail").Select(x => $"{x.Name}: {x.Detail}")))
            .IsEqualTo("");
        await Assert.That(report.Results.Single(x => x.Name == "Pagination").Status).IsEqualTo("pass");
        await Assert.That(report.Results.Single(x => x.Name == "TLS certificate and handshake").Status)
            .IsEqualTo("pass");
    }

    [Test]
    public async Task RepeatedCursorPageIsReportedAsFailure()
    {
        var runner =
            CreateRunner(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Ban12) });
        var report = await runner.RunAsync(SourceType.Standard, new Uri("http://example.org/"));

        await Assert.That(report.Results.Single(x => x.Name == "Pagination").Status).IsEqualTo("fail");
        await Assert.That(report.Results.Single(x => x.Name == "TLS certificate and handshake").Status)
            .IsEqualTo("skip");
        await Assert.That(report.Results.Single(x => x.Name == "Pagination").Error).IsNotNull();
    }

    [Test]
    public async Task HttpFailureAndMalformedPayloadArePreservedInReport()
    {
        var runner = CreateRunner(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("offline")
        });
        var report = await runner.RunAsync(SourceType.Standard, new Uri("https://example.org/"));

        await Assert.That(report.Results.Single(x => x.Name == "HTTP connection and endpoint").Status)
            .IsEqualTo("fail");
        await Assert.That(report.Results.Single(x => x.Name == "TLS certificate and handshake").Status)
            .IsEqualTo("pass");
        await Assert.That(report.Results.Single(x => x.Name == "Ban response parsing").Status).IsEqualTo("fail");
        await Assert.That(report.Results.Single(x => x.Name == "Pagination").Status).IsEqualTo("skip");
        await Assert.That(report.Results.Single(x => x.Name == "Ban response parsing").Error).IsNotNull();
        await Assert.That(report.Results.Single(x => x.Name == "Ban response parsing").Error).Contains("offline");
    }

    [Test]
    public async Task OnePageSourceDoesNotRequestNonexistentSecondPage()
    {
        const string beePage =
            """{"pages":1,"data":[{"unbanned_datetime":null,"expiration_time":null,"bantime":"2024-01-01T00:00:00Z","a_ckey":"admin","unbanned_ckey":null,"roles":["Server"],"ckey":"alice","reason":"test","id":3,"server_name":"bs_golden","global_ban":0}]}""";
        var requestedSecondPage = false;
        var runner = CreateRunner(request =>
        {
            if (request.RequestUri!.Query.Contains("page=2")) requestedSecondPage = true;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(beePage) };
        });

        var report = await runner.RunAsync(SourceType.Bee, new Uri("https://example.org/"));

        await Assert.That(report.Results.Single(x => x.Name == "Pagination metadata").Status).IsEqualTo("pass");
        await Assert.That(report.Results.Single(x => x.Name == "Ban data integrity").Status).IsEqualTo("pass");
        await Assert.That(report.Results.Single(x => x.Name == "Pagination").Status).IsEqualTo("skip");
        await Assert.That(requestedSecondPage).IsFalse();
    }

    [Test]
    public async Task TimeoutIsIncludedInReport()
    {
        var runner = new EndpointTests(() => new TimeoutHandler());
        var report = await runner.RunAsync(SourceType.Standard, new Uri("https://example.org/"));

        await Assert.That(report.Results.Single(x => x.Name == "HTTP connection and endpoint").Status)
            .IsEqualTo("fail");
        await Assert.That(report.Results.Single(x => x.Name == "TLS certificate and handshake").Status)
            .IsEqualTo("fail");
        await Assert.That(report.Results.Single(x => x.Name == "Ban response parsing").Error).IsNotNull();
    }

    [Test]
    public async Task ProviderWithoutBanIdsDetectsRepeatedPages()
    {
        const string page =
            """{"value":{"lastPage":2,"bans":[{"unbannedTime":null,"banExpireTime":null,"banApplyTime":"2024-01-01T00:00:00Z","adminCkey":"admin","role":["server"],"bannedCkey":"alice","reason":"test"}]}}""";
        var runner = CreateRunner(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(page)
        });

        var report = await runner.RunAsync(SourceType.Fulp, new Uri("https://example.org/"));

        await Assert.That(report.Results.Single(x => x.Name == "Ban data integrity").Status).IsEqualTo("pass");
        await Assert.That(report.Results.Single(x => x.Name == "Pagination").Status).IsEqualTo("fail");
    }

    [Test]
    public async Task EmptyFirstPageWithMultipleReportedPagesFailsPagination()
    {
        var runner = CreateRunner(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"pages":2,"data":[]}""")
        });

        var report = await runner.RunAsync(SourceType.Bee, new Uri("https://example.org/"));

        await Assert.That(report.Results.Single(x => x.Name == "Ban response parsing").Status).IsEqualTo("pass");
        await Assert.That(report.Results.Single(x => x.Name == "Pagination").Status).IsEqualTo("fail");
    }

    [Test]
    public async Task NonemptyFirstPageWithZeroReportedPagesFailsMetadataAndPagination()
    {
        const string beePage =
            """{"pages":0,"data":[{"unbanned_datetime":null,"expiration_time":null,"bantime":"2024-01-01T00:00:00Z","a_ckey":"admin","unbanned_ckey":null,"roles":["Server"],"ckey":"alice","reason":"test","id":3,"server_name":"bs_golden","global_ban":0}]}""";
        var requestedSecondPage = false;
        var runner = CreateRunner(request =>
        {
            if (request.RequestUri!.Query.Contains("page=2")) requestedSecondPage = true;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(beePage) };
        });

        var report = await runner.RunAsync(SourceType.Bee, new Uri("https://example.org/"));

        await Assert.That(report.Results.Single(x => x.Name == "Ban response parsing").Status).IsEqualTo("pass");
        await Assert.That(report.Results.Single(x => x.Name == "Pagination metadata").Status).IsEqualTo("fail");
        await Assert.That(report.Results.Single(x => x.Name == "Pagination").Status).IsEqualTo("fail");
        await Assert.That(requestedSecondPage).IsFalse();
    }

    [Test]
    public async Task CanonicalEndpointRedirectWorksLikeProductionClient()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        await using var app = builder.Build();
        app.MapGet("/source/api/ban", (HttpRequest request) =>
            Results.Redirect("/canonical/api/ban" + request.QueryString));
        app.MapGet("/canonical/api/ban", (HttpRequest request) =>
            Results.Text(request.Query.ContainsKey("cursor") ? Ban11 : Ban12, "application/json"));
        await app.StartAsync();

        var report = await new EndpointTests().RunAsync(SourceType.Standard, new Uri(app.Urls.Single() + "/source/"));

        await Assert.That(report.Results.Single(x => x.Name == "HTTP connection and endpoint").Status)
            .IsEqualTo("pass");
        await Assert.That(report.Results.Single(x => x.Name == "Ban response parsing").Status).IsEqualTo("pass");
        await Assert.That(report.Results.Single(x => x.Name == "Pagination").Status).IsEqualTo("pass");
    }

    [Test]
    public async Task DisconnectCancelsProviderRequest()
    {
        using var cancellation = new CancellationTokenSource();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requests = 0;
        var runner = new EndpointTests(() => new AsyncHandler(async token =>
        {
            if (Interlocked.Increment(ref requests) == 1)
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Ban12) };
            started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new InvalidOperationException("Unreachable");
        }));

        var run = runner.RunAsync(SourceType.Standard, new Uri("http://example.org/"), cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(3));
        cancellation.Cancel();
        var canceled = false;
        try
        {
            await run.WaitAsync(TimeSpan.FromSeconds(3));
        }
        catch (OperationCanceledException)
        {
            canceled = true;
        }

        await Assert.That(canceled).IsTrue();
    }

    [Test]
    public async Task DisconnectCancelsVgFetch()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        await using var app = builder.Build();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requests = 0;
        app.MapGet("/index.php/bans", async (HttpContext context) =>
        {
            if (Interlocked.Increment(ref requests) == 1)
                return Results.Text("<html></html>", "text/html");
            started.TrySetResult();
            await Task.Delay(TimeSpan.FromSeconds(5), context.RequestAborted);
            return Results.Text("<html></html>", "text/html");
        });
        await app.StartAsync();

        using var cancellation = new CancellationTokenSource();
        var run = new EndpointTests().RunAsync(SourceType.Vg, new Uri(app.Urls.Single() + "/"), cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(3));
        cancellation.Cancel();
        var canceled = false;
        try
        {
            await run.WaitAsync(TimeSpan.FromSeconds(3));
        }
        catch (OperationCanceledException)
        {
            canceled = true;
        }

        await Assert.That(canceled).IsTrue();
    }

    [Test]
    public async Task DisconnectCancelsResponseBodyRead()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        await using var app = builder.Build();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requests = 0;
        app.MapGet("/api/ban", async (HttpContext context) =>
        {
            context.Response.ContentType = "application/json";
            if (Interlocked.Increment(ref requests) == 1)
            {
                await context.Response.WriteAsync(Ban12);
                return;
            }

            await context.Response.WriteAsync("[");
            await context.Response.Body.FlushAsync();
            started.TrySetResult();
            await Task.Delay(TimeSpan.FromSeconds(5), context.RequestAborted);
        });
        await app.StartAsync();

        using var cancellation = new CancellationTokenSource();
        var run = new EndpointTests().RunAsync(SourceType.Standard, new Uri(app.Urls.Single() + "/"),
            cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(3));
        cancellation.Cancel();
        var canceled = false;
        try
        {
            await run.WaitAsync(TimeSpan.FromSeconds(3));
        }
        catch (OperationCanceledException)
        {
            canceled = true;
        }

        await Assert.That(canceled).IsTrue();
    }

    private static EndpointTests CreateRunner(Func<HttpRequestMessage, HttpResponseMessage> respond) =>
        new(() => new StubHandler(respond));

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }

    private sealed class TimeoutHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(new TaskCanceledException("Request timed out."));
    }

    private sealed class AsyncHandler(Func<CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => respond(cancellationToken);
    }
}