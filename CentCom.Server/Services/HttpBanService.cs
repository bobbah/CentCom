using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CentCom.Server.Exceptions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.Services;

public abstract class HttpBanService
{
    private readonly ILogger<HttpBanService> _logger;
    private readonly HttpClient _httpClient;

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
        if (BaseUrl != null)
            _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            $"Mozilla/5.0 (compatible; CentComBot/{Assembly.GetExecutingAssembly().GetName().Version}; +https://centcom.melonmesa.com/scraper)");
    }

    protected void SetBaseAddress(string address) => _httpClient.BaseAddress = new Uri(address);

    protected async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null,
        JsonSerializerOptions? options = null) =>
        JsonSerializer.Deserialize<T>(await GetAsStringAsync(endpoint, queryParams), options ?? JsonOptions);

    protected async Task<string> GetAsStringAsync(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        var url = queryParams is not null ? QueryHelpers.AddQueryString(endpoint, queryParams) : endpoint;
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            await FailedRequest(response);

        return await response.Content.ReadAsStringAsync();
    }

    protected async Task FailedRequest(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        _logger.LogError(
            "Source website returned a non-200 HTTP response code.\n\tCode: {ResponseCode}\n\tRequest URL: \"{RequestUrl}\"",
            response.StatusCode, response.RequestMessage?.RequestUri);
        throw new BanSourceUnavailableException(
            $"Source website returned a non-200 HTTP response code.\n\tCode: {response.StatusCode}\n\tRequest URL: \"{response.RequestMessage?.RequestUri}\"",
            content);
    }
}