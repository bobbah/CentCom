using System.Net;
using System.Text.Json;
using CentCom.Server.Configuration;
using CentCom.Server.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace CentCom.Test.BanServices;

public class StandardProviderPagingTests
{
    [Test]
    public async Task IncrementalFetchContinuesUntilKnownBanAndKeepsEntireBoundaryPage()
    {
        var handler = new PagingHandler(cursor => cursor switch
        {
            null => [110, 109],
            109 => [108, 107, 106],
            _ => throw new InvalidOperationException($"Unexpected cursor: {cursor}")
        });
        var bans = await CreateService(handler).GetBansBatchedAsync(searchFor: [107]);

        await Assert.That(string.Join(",", bans.Select(x => x.BanID)))
            .IsEqualTo("110,109,108,107,106");
        await Assert.That(string.Join(",", handler.RequestedCursors.Select(x => x?.ToString() ?? "start")))
            .IsEqualTo("start,109");
    }

    [Test]
    public async Task IncrementalFetchWithoutKnownBanContinuesUntilEmptyPage()
    {
        var handler = new PagingHandler(cursor => cursor switch
        {
            null => [9, 8],
            8 => [7],
            7 => [],
            _ => throw new InvalidOperationException($"Unexpected cursor: {cursor}")
        });
        var bans = await CreateService(handler).GetBansBatchedAsync(searchFor: [6]);

        await Assert.That(string.Join(",", bans.Select(x => x.BanID))).IsEqualTo("9,8,7");
        await Assert.That(string.Join(",", handler.RequestedCursors.Select(x => x?.ToString() ?? "start")))
            .IsEqualTo("start,8,7");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task FullRefreshAndEmptySearchFetchAllPages(bool emptySearch)
    {
        var handler = new PagingHandler(cursor => cursor switch
        {
            null => [5, 4],
            4 => [3],
            3 => [],
            _ => throw new InvalidOperationException($"Unexpected cursor: {cursor}")
        });
        var service = CreateService(handler);
        var bans = emptySearch
            ? await service.GetBansBatchedAsync(searchFor: [])
            : await service.GetBansBatchedAsync();

        await Assert.That(string.Join(",", bans.Select(x => x.BanID))).IsEqualTo("5,4,3");
        await Assert.That(string.Join(",", handler.RequestedCursors.Select(x => x?.ToString() ?? "start")))
            .IsEqualTo("start,4,3");
    }

    [Test]
    public async Task NonAdvancingCursorFailsInsteadOfRequestingSamePageForever()
    {
        var handler = new PagingHandler(_ => [7, 6]);
        var service = CreateService(handler);

        await Assert.That(() => service.GetBansBatchedAsync()).Throws<InvalidOperationException>();
        await Assert.That(string.Join(",", handler.RequestedCursors.Select(x => x?.ToString() ?? "start")))
            .IsEqualTo("start,6");
    }

    [Test]
    public async Task CursorMovingBackwardsFromExplicitStartingPointFails()
    {
        var handler = new PagingHandler(_ => [11]);
        var service = CreateService(handler);

        await Assert.That(() => service.GetBansBatchedAsync(cursor: 10))
            .Throws<InvalidOperationException>();
        await Assert.That(string.Join(",", handler.RequestedCursors.Select(x => x?.ToString() ?? "start")))
            .IsEqualTo("10");
    }

    private static StandardProviderService CreateService(PagingHandler handler)
    {
        var service = new StandardProviderService(new HttpClient(handler),
            NullLogger<StandardProviderService>.Instance);
        service.Configure(new StandardProviderConfiguration
        {
            Id = "test", Display = "Test", Url = "https://exporter.example/"
        });
        return service;
    }

    private sealed class PagingHandler(Func<int?, int[]> getPage) : HttpMessageHandler
    {
        public List<int?> RequestedCursors { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath != "/api/ban")
                throw new InvalidOperationException($"Unexpected endpoint: {request.RequestUri}");

            var query = request.RequestUri.Query;
            if (query.Length > 0 && !query.StartsWith("?cursor="))
                throw new InvalidOperationException($"Unexpected query: {query}");

            int? cursor = query.Length == 0 ? null : int.Parse(query["?cursor=".Length..]);
            RequestedCursors.Add(cursor);
            var bans = getPage(cursor).Select(id => new
            {
                id,
                banType = "Server",
                cKey = "test",
                bannedOn = DateTimeOffset.UnixEpoch,
                bannedBy = "admin",
                reason = "test",
                expires = (DateTimeOffset?)null,
                unbannedBy = (string?)null,
                jobBans = Array.Empty<string>()
            });
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(bans))
            });
        }
    }
}
