using System.Diagnostics;
using System.Net;
using CentCom.Common.Models;
using CentCom.Server.Configuration;
using CentCom.Server.Exceptions;
using CentCom.Server.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace CentCom.Detective;

public enum SourceType
{
    Standard,
    Bee,
    Fulp,
    Tg,
    Tgmc,
    Yog,
    Vg
}

public record TestRequest(string Type, string Address);

public record TestResult(string Name, string Status, double Milliseconds, string Detail, string? Error = null);

public record TestReport(DateTimeOffset StartedAt, string Source, string Address, List<TestResult> Results);

public sealed class EndpointTests
{
    private readonly Func<HttpMessageHandler> _handlerFactory;

    public EndpointTests() : this(() => new HttpClientHandler())
    {
    }

    public EndpointTests(Func<HttpMessageHandler> handlerFactory) => _handlerFactory = handlerFactory;

    public async Task<TestReport> RunAsync(SourceType type, Uri address, CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        var results = new List<TestResult>();
        var baseUrl = address.AbsoluteUri.TrimEnd('/') + "/";
        var baseAddress = new Uri(baseUrl);
        var path = type switch
        {
            SourceType.Standard => "api/ban",
            SourceType.Bee or SourceType.Yog => "bans",
            SourceType.Fulp => "bans/50/1",
            SourceType.Tg => "bans/public/v1/1?json=true",
            SourceType.Tgmc => "bans/1?limit=100",
            SourceType.Vg => "index.php/bans",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        using var client = new HttpClient(new CancellationHandler(_handlerFactory(), cancellationToken))
        {
            BaseAddress = baseAddress,
            Timeout = TimeSpan.FromSeconds(15)
        };
        using var cancelPending = cancellationToken.Register(client.CancelPendingRequests);
        using var probeClient = new HttpClient(_handlerFactory()) { Timeout = TimeSpan.FromSeconds(15) };
        var endpoint = new Uri(baseAddress, path);
        var receivedResponse = false;
        var transport = await CheckAsync("HTTP connection and endpoint", async () =>
        {
            using var response =
                await probeClient.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            receivedResponse = true;
            if (response.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestException(
                    $"Expected HTTP 200, received {(int)response.StatusCode} {response.ReasonPhrase}.");
            return $"HTTP {(int)response.StatusCode} at {endpoint}";
        }, cancellationToken);
        results.Add(transport);
        results.Add(address.Scheme == "https"
            ? new TestResult("TLS certificate and handshake", receivedResponse ? "pass" : "fail",
                transport.Milliseconds,
                receivedResponse
                    ? "HTTPS handshake and certificate validation succeeded (no certificate bypass)."
                    : "No HTTPS response received; see HTTP connection error.",
                receivedResponse ? null : transport.Error)
            : new TestResult("TLS certificate and handshake", "skip", 0,
                "Not applicable to HTTP. Use HTTPS to validate TLS."));

        List<Ban>? first = null;
        var parsed = await CheckAsync("Ban response parsing", async () =>
        {
            first = await GetPageAsync(type, client, baseAddress, 1, cancellationToken);
            return $"Parsed {first.Count} bans from page 1 using the CentCom provider.";
        }, cancellationToken);
        results.Add(parsed);

        int? pages = null;
        if (type is SourceType.Bee or SourceType.Fulp or SourceType.Tgmc)
            results.Add(await CheckAsync("Pagination metadata", async () =>
            {
                pages = type switch
                {
                    SourceType.Bee => await new BeeBanService(client, NullLogger<BeeBanService>.Instance)
                        .GetNumberOfPagesAsync(cancellationToken),
                    SourceType.Fulp => await new FulpBanService(client, NullLogger<FulpBanService>.Instance)
                        .GetNumberOfPagesAsync(cancellationToken),
                    _ => await new TGMCBanService(client, NullLogger<TGMCBanService>.Instance)
                        .GetNumberOfPagesAsync(cancellationToken)
                };
                if (pages < 0) throw new InvalidDataException($"Negative page count: {pages}.");
                if (pages == 0 && first?.Count > 0)
                    throw new InvalidDataException("Source reports zero pages, but page 1 contains bans.");
                return $"Source reports {pages} page(s).";
            }, cancellationToken));

        if (first is null)
        {
            results.Add(new TestResult("Ban data integrity", "skip", 0, "Parsing failed; no bans to inspect."));
            results.Add(new TestResult("Pagination", "skip", 0, "Parsing failed; cannot probe another page."));
        }
        else
        {
            results.Add(first.Count == 0
                ? new TestResult("Ban data integrity", "skip", 0, "Empty page; no ban records to inspect.")
                : await CheckAsync("Ban data integrity", () =>
                {
                    ValidateBans(type, first);
                    return Task.FromResult($"All {first.Count} bans contain required fields.");
                }, cancellationToken));

            if (type == SourceType.Vg)
                results.Add(new TestResult("Pagination", "skip", 0, "This source has a single HTML ban page."));
            else if (first.Count == 0 && pages > 1)
                results.Add(new TestResult("Pagination", "fail", 0,
                    $"Source reports {pages} pages, but page 1 is empty."));
            else if (first.Count == 0)
                results.Add(new TestResult("Pagination", "skip", 0, "Empty first page; cannot confirm the next page."));
            else if (pages == 0)
                results.Add(new TestResult("Pagination", "fail", 0,
                    "Source reports zero pages, but page 1 contains bans."));
            else if (pages == 1)
                results.Add(new TestResult("Pagination", "skip", 0,
                    "Source reports only one page; no second page to compare."));
            else
                results.Add(await CheckAsync("Pagination", async () =>
                {
                    var second = await GetPageAsync(type, client, baseAddress, 2, cancellationToken,
                        type == SourceType.Standard ? int.Parse(first[^1].BanID!) : null);
                    ValidateBans(type, second);
                    if (pages > 1 && second.Count == 0)
                        throw new InvalidDataException($"Source reports {pages} pages, but page 2 is empty.");
                    var firstKeys = first.Select(BanKey).ToHashSet();
                    var duplicates = second.Count(b => firstKeys.Contains(BanKey(b)));
                    if (duplicates > 0)
                        throw new InvalidDataException($"{duplicates} bans are repeated on page 2.");
                    if (type == SourceType.Standard &&
                        second.Any(b => int.Parse(b.BanID!) >= int.Parse(first[^1].BanID!)))
                        throw new InvalidDataException("Cursor page includes bans at or above the exclusive cursor.");
                    return $"Page 2 parsed {second.Count} bans; no overlapping records detected.";
                }, cancellationToken));
        }

        return new TestReport(started, type.ToString(), baseUrl, results);
    }

    private static string BanKey(Ban ban) => ban.BanID ??
                                             $"{ban.CKey}|{ban.BannedOn:O}|{ban.BannedBy}|{ban.BanType}|{string.Join(",", ban.JobBans?.Select(j => j.Job).Order() ?? Enumerable.Empty<string>())}";

    private static void ValidateBans(SourceType type, List<Ban> bans)
    {
        var invalid = bans.Count(b => string.IsNullOrWhiteSpace(b.CKey) ||
                                      b.BannedOn == default ||
                                      b.SourceNavigation is null ||
                                      (type is SourceType.Standard or SourceType.Bee or SourceType.Tg or SourceType.Tgmc
                                           or SourceType.Yog &&
                                       string.IsNullOrWhiteSpace(b.BanID)) ||
                                      (b.BanType == BanType.Job && (b.JobBans is null || b.JobBans.Count == 0)));
        if (invalid > 0)
            throw new InvalidDataException(
                $"{invalid} of {bans.Count} bans are missing a ckey, date, source, required ban ID or job roles.");
    }

    private static async Task<List<Ban>> GetPageAsync(SourceType type, HttpClient client, Uri baseAddress, int page,
        CancellationToken cancellationToken, int? cursor = null)
    {
        return type switch
        {
            SourceType.Standard => await StandardPage(client, baseAddress, cursor, cancellationToken),
            SourceType.Bee => await new BeeBanService(client, NullLogger<BeeBanService>.Instance)
                .GetBansAsync(page, cancellationToken),
            SourceType.Fulp => await new FulpBanService(client, NullLogger<FulpBanService>.Instance)
                .GetBansAsync(page, cancellationToken),
            SourceType.Tg => (await new TgBanService(client, NullLogger<TgBanService>.Instance)
                    .GetBansAsync(page, cancellationToken))
                .Select(b => b.AsBan(new BanSource { Name = "tgstation" })).ToList(),
            SourceType.Tgmc => await new TGMCBanService(client, NullLogger<TGMCBanService>.Instance)
                .GetBansAsync(page, cancellationToken),
            SourceType.Yog => await new YogBanService(client, NullLogger<YogBanService>.Instance)
                .GetBansAsync(page, cancellationToken),
            SourceType.Vg => await GetVgPageAsync(baseAddress, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private static async Task<List<Ban>> GetVgPageAsync(Uri baseAddress, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        return await new VgBanService(NullLogger<VgBanService>.Instance)
            .GetBansAsync(new Uri(baseAddress, "index.php/bans").AbsoluteUri, timeout.Token);
    }

    private static async Task<List<Ban>> StandardPage(HttpClient client, Uri address, int? cursor,
        CancellationToken cancellationToken)
    {
        var service = new StandardProviderService(client, NullLogger<StandardProviderService>.Instance);
        service.Configure(new StandardProviderConfiguration
        {
            Url = address.AbsoluteUri, Id = "detective", Display = "Detective"
        });
        return await service.GetBansAsync(cursor, cancellationToken);
    }

    private static async Task<TestResult> CheckAsync(string name, Func<Task<string>> check,
        CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var detail = await check();
            return new TestResult(name, "pass", timer.Elapsed.TotalMilliseconds, detail);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            var response = ex is BanSourceUnavailableException sourceError
                ? $"\nResponse excerpt: {sourceError.ResponseContent[..Math.Min(sourceError.ResponseContent.Length, 2000)]}"
                : "";
            return new TestResult(name, "fail", timer.Elapsed.TotalMilliseconds, ex.Message, ex + response);
        }
    }

    private sealed class CancellationHandler(HttpMessageHandler inner, CancellationToken cancellationToken)
        : DelegatingHandler(inner)
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken requestToken)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(requestToken, cancellationToken);
            return await base.SendAsync(request, linked.Token);
        }
    }
}