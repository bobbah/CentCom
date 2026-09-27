using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CentCom.Server.Exceptions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.Services;

public abstract class HttpBanService
{
    private const string TruncationMarker = "\n[response truncated]";
    private readonly ILogger<HttpBanService> _logger;
    private readonly HttpClient _httpClient;

    protected virtual int MaxFailureBodyCharacters => 8 * 1024;
    protected virtual int MaxSuccessBodyCharacters => 32 * 1024 * 1024;

    public virtual JsonSerializerOptions JsonOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    protected HttpBanService(HttpClient httpClient, ILogger<HttpBanService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        ConfigureClient();
    }
    
    protected abstract string BaseUrl { get; }

    protected void ConfigureClient()
    {
        if (_httpClient.BaseAddress == null && BaseUrl != null)
            _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            $"Mozilla/5.0 (compatible; CentComBot/{Assembly.GetExecutingAssembly().GetName().Version}; +https://centcom.melonmesa.com/scraper)");
    }

    protected void SetBaseAddress(string address)
    {
        var uri = new Uri(address);
        if (_httpClient.BaseAddress != uri)
            _httpClient.BaseAddress = uri;
    }

    protected async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string> queryParams = null,
        JsonSerializerOptions options = null, CancellationToken cancellationToken = default) =>
        JsonSerializer.Deserialize<T>(await GetAsStringAsync(endpoint, queryParams, cancellationToken),
            options ?? JsonOptions);

    protected async Task<string> GetAsStringAsync(string endpoint, Dictionary<string, string> queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var url = queryParams is not null ? QueryHelpers.AddQueryString(endpoint, queryParams) : endpoint;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (_httpClient.Timeout != Timeout.InfiniteTimeSpan)
            timeout.CancelAfter(_httpClient.Timeout);
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        if (!response.IsSuccessStatusCode)
            await FailedRequest(response, timeout.Token);

        var content = await ReadBodyAsync(response.Content, MaxSuccessBodyCharacters, timeout.Token,
            retainTruncatedText: false);
        if (content.Truncated)
            throw new BanSourceUnavailableException(
                $"Source website response exceeded the {MaxSuccessBodyCharacters} character limit.",
                string.Empty);

        return content.Text;
    }

    protected async Task FailedRequest(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var content = await ReadBodyAsync(response.Content, MaxFailureBodyCharacters, cancellationToken);
        _logger.LogError(
            "Source website returned a non-200 HTTP response code.\n\tCode: {ResponseCode}\n\tRequest URL: \"{RequestUrl}\"",
            response.StatusCode, response.RequestMessage?.RequestUri);
        throw new BanSourceUnavailableException(
            $"Source website returned a non-200 HTTP response code.\n\tCode: {response.StatusCode}\n\tRequest URL: \"{response.RequestMessage?.RequestUri}\"",
            content.Text + (content.Truncated ? TruncationMarker : string.Empty));
    }

    private static async Task<(string Text, bool Truncated)> ReadBodyAsync(HttpContent content, int maxCharacters,
        CancellationToken cancellationToken, bool retainTruncatedText = true)
    {
        if (maxCharacters < 0)
            throw new ArgumentOutOfRangeException(nameof(maxCharacters));

        var charset = content.Headers.ContentType?.CharSet?.Trim('"');
        var encoding = charset is null ? Encoding.UTF8 : Encoding.GetEncoding(charset);
        using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: charset is null);
        var result = new StringBuilder();
        var buffer = new char[4096];
        while (true)
        {
            var read = await reader.ReadAsync(buffer.AsMemory(0,
                Math.Min(buffer.Length, maxCharacters - result.Length + 1)), cancellationToken);
            if (read == 0)
                return (result.ToString(), false);
            var retained = Math.Min(read, maxCharacters - result.Length);
            result.Append(buffer, 0, retained);
            if (retained < read)
                return (retainTruncatedText ? result.ToString() : string.Empty, true);
        }
    }
}