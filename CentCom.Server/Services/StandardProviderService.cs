using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CentCom.Common.Extensions;
using CentCom.Common.Models;
using CentCom.Common.Models.Rest;
using CentCom.Server.Configuration;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.Services;

public class StandardProviderService(HttpClient client, ILogger<StandardProviderService> logger)
    : HttpBanService(client, logger)
{
    private string _baseUrl;
    private bool _configured;

    public BanSource Source { get; private set; }
    protected override string BaseUrl => _baseUrl;

    public override JsonSerializerOptions JsonOptions => new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    }.AddCentComOptions();

    private async Task<List<Ban>> GetBansAsync(int? cursor = null)
    {
        var data = await GetAsync<List<RestBan>>("api/ban",
            cursor.HasValue ? new Dictionary<string, string>() { { "cursor", cursor.ToString() } } : null, JsonOptions);
        
        return data.Select(x => new Ban
        {
            BanID = x.Id.ToString(),
            BannedBy = x.BannedBy?.CanonicalKey,
            BannedOn = x.BannedOn.UtcDateTime,
            BanType = x.BanType,
            CKey = x.CKey?.CanonicalKey,
            Expires = x.Expires?.UtcDateTime,
            UnbannedBy = x.UnbannedBy?.CanonicalKey,
            Reason = x.Reason,
            JobBans = x.JobBans?.Select(j => new JobBan
                {
                    Job = j.Job
                })
                .ToHashSet(),
            SourceNavigation = Source
        }).ToList();
    }

    public async Task<List<Ban>> GetBansBatchedAsync(int? cursor = null, List<int> searchFor = null)
    {
        if (!_configured)
            throw new Exception("Cannot get bans from an unconfigured external source");

        // Get bans, must use a sequential approach here due to the last ban
        // ID only being available once we finish a page
        var result = new List<Ban>();
        var lastRequested = cursor;
        List<Ban> lastResponse;
        do
        {
            lastResponse = (await GetBansAsync(lastRequested)).ToList();
            if (lastResponse.Count == 0)
                break;
            lastRequested = int.Parse(lastResponse[^1].BanID);
            result.AddRange(lastResponse);
        } while (lastResponse.Count != 0 &&
                 (searchFor == null || lastResponse.Any(x => searchFor.Contains(int.Parse(x.BanID)))));

        return result;
    }

    public void Configure(StandardProviderConfiguration config)
    {
        if (_configured)
            throw new Exception("Cannot re-configure standard exporter provider, already configured");
        _configured = true;
        _baseUrl = config.Url;
        SetBaseAddress(_baseUrl);
        Source = new BanSource { Name = config.Id, Display = config.Display, RoleplayLevel = config.RoleplayLevel };
    }
}