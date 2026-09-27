using System.Net;
using System.Text;
using CentCom.Server.Exceptions;
using CentCom.Server.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace CentCom.Test.BanServices;

public class HttpBanServiceTests
{
    [Test]
    public async Task SuccessfulJsonIsReadInFullAndResponseIsDisposed()
    {
        var payload = $$"""{"value":"{{new string('a', 12_000)}}"}""";
        var content = new TrackedContent(payload);
        using var client = CreateClient((request, _) =>
        {
            if (request.RequestUri!.Query != "?page=2")
                throw new InvalidOperationException("Query parameters were not forwarded.");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        });
        var service = new TestService(client, successLimit: payload.Length);

        var result = await service.FetchJsonAsync("bans", new Dictionary<string, string> { ["page"] = "2" });

        await Assert.That(result.Value).IsEqualTo(new string('a', 12_000));
        await Assert.That(content.WasDisposed).IsTrue();
    }

    [Test]
    public async Task SuccessOverLimitFailsInsteadOfReturningTruncatedData()
    {
        var content = new TrackedContent("123456789");
        using var client = CreateClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content }));
        var service = new TestService(client, successLimit: 8);

        var error = await CaptureUnavailable(() => service.FetchAsync("bans"));

        await Assert.That(error.Message).Contains("8 character limit");
        await Assert.That(error.ResponseContent).IsEmpty();
        await Assert.That(content.WasDisposed).IsTrue();
    }

    [Test]
    public async Task SuccessRespectsDeclaredCharset()
    {
        using var client = CreateClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("café", Encoding.Latin1)
            }));

        var result = await new TestService(client, successLimit: 4).FetchAsync("bans");

        await Assert.That(result).IsEqualTo("café");
    }

    [Test]
    public async Task ShortFailureRetainsWholeBodyAndDisposesResponse()
    {
        var content = new TrackedContent("not found");
        using var client = CreateClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = content }));
        var service = new TestService(client, failureLimit: 9);

        var error = await CaptureUnavailable(() => service.FetchAsync("bans"));

        await Assert.That(error.ResponseContent).IsEqualTo("not found");
        await Assert.That(content.WasDisposed).IsTrue();
    }

    [Test]
    public async Task LargeFailureReadsOnlyAWindowAndMarksPersistedText()
    {
        var stream = new ObservedStream(Encoding.UTF8.GetBytes(new string('x', 1_000_000)));
        var content = new TrackedStreamContent(stream);
        using var client = CreateClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway) { Content = content }));
        var service = new TestService(client, failureLimit: 64);

        var error = await CaptureUnavailable(() => service.FetchAsync("bans"));

        await Assert.That(error.ResponseContent).StartsWith(new string('x', 64));
        await Assert.That(error.ResponseContent).Contains("[response truncated]");
        await Assert.That(error.ResponseContent.Length).IsLessThan(100);
        await Assert.That(stream.BytesRead).IsGreaterThan(64);
        await Assert.That(stream.BytesRead).IsLessThan(10_000);
        await Assert.That(content.WasDisposed).IsTrue();
    }

    [Test]
    public async Task Utf8FailureIsLimitedByCharactersNotPartialBytes()
    {
        var content = new TrackedContent("ééé");
        using var client = CreateClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = content }));
        var service = new TestService(client, failureLimit: 2);

        var error = await CaptureUnavailable(() => service.FetchAsync("bans"));

        await Assert.That(error.ResponseContent).StartsWith("éé");
        await Assert.That(error.ResponseContent).Contains("[response truncated]");
    }

    [Test]
    public async Task CancellationDuringBodyReadPropagatesAndDisposesResponse()
    {
        using var cancellation = new CancellationTokenSource();
        var stream = new BlockingStream();
        var content = new TrackedStreamContent(stream);
        using var client = CreateClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content }));
        var service = new TestService(client);

        var request = service.FetchAsync("bans", cancellation.Token);
        await stream.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();

        await Assert.That(async () => { await request; }).Throws<OperationCanceledException>();
        await Assert.That(content.WasDisposed).IsTrue();
    }

    [Test]
    public async Task CancellationDuringSendPropagates()
    {
        using var cancellation = new CancellationTokenSource();
        var sendStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var client = CreateClient(async (_, token) =>
        {
            sendStarted.TrySetResult();
            await Task.Delay(Timeout.Infinite, token);
            throw new InvalidOperationException("The send should have been canceled.");
        });
        var service = new TestService(client);

        var request = service.FetchAsync("bans", cancellation.Token);
        await sendStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();

        await Assert.That(async () => { await request; }).Throws<OperationCanceledException>();
    }

    [Test]
    public async Task ClientTimeoutStillAppliesToStreamingBody()
    {
        var stream = new BlockingStream();
        var content = new TrackedStreamContent(stream);
        using var client = CreateClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content }));
        client.Timeout = TimeSpan.FromMilliseconds(200);

        var request = new TestService(client).FetchAsync("bans");
        await stream.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(async () => { await request.WaitAsync(TimeSpan.FromSeconds(5)); })
            .Throws<OperationCanceledException>();
        await Assert.That(content.WasDisposed).IsTrue();
    }

    private static async Task<BanSourceUnavailableException> CaptureUnavailable(Func<Task<string>> action)
    {
        try
        {
            await action();
        }
        catch (BanSourceUnavailableException error)
        {
            return error;
        }

        throw new InvalidOperationException("Expected BanSourceUnavailableException.");
    }

    private static HttpClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) =>
        new(new StubHandler(respond));

    private sealed class TestService(HttpClient client, int failureLimit = 8 * 1024,
        int successLimit = 32 * 1024 * 1024) : HttpBanService(client, NullLogger<HttpBanService>.Instance)
    {
        protected override string BaseUrl => "https://example.test/";
        protected override int MaxFailureBodyCharacters => failureLimit;
        protected override int MaxSuccessBodyCharacters => successLimit;

        public Task<string> FetchAsync(string endpoint, CancellationToken cancellationToken = default) =>
            GetAsStringAsync(endpoint, cancellationToken: cancellationToken);

        public Task<TestJson> FetchJsonAsync(string endpoint, Dictionary<string, string> query) =>
            GetAsync<TestJson>(endpoint, query);
    }

    private sealed class TestJson
    {
        public string Value { get; set; } = "";
    }

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => respond(request, cancellationToken);
    }

    private sealed class TrackedContent(string text) : StringContent(text)
    {
        public bool WasDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class TrackedStreamContent(Stream stream) : StreamContent(stream)
    {
        public bool WasDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class ObservedStream(byte[] bytes) : MemoryStream(bytes)
    {
        public int BytesRead { get; private set; }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var read = base.ReadAsync(buffer, cancellationToken);
            if (read.IsCompletedSuccessfully)
                BytesRead += read.Result;
            return read;
        }
    }

    private sealed class BlockingStream : Stream
    {
        public TaskCompletionSource ReadStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ReadStarted.TrySetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return 0;
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
