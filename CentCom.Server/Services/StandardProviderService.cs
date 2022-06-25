using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CentCom.Common.Abstract;
using CentCom.Common.Extensions;
using CentCom.Common.Models;
using CentCom.Common.Models.Rest;
using CentCom.Server.Configuration;
using Microsoft.Extensions.Logging;
using Remora.Rest.Extensions;
using RestSharp;
using RestSharp.Serializers.Json;

namespace CentCom.Server.Services;

public class StandardProviderService : RestBanService
{
    private string _baseUrl;
    private bool _configured;

    public StandardProviderService(ILogger<StandardProviderService> logger) : base(logger)
    {
    }

    public BanSource Source { get; private set; }
    protected override string BaseUrl => _baseUrl;

    private async Task<IEnumerable<Ban>> GetBansAsync(int? cursor = null)
    {
        var request = new RestRequest("api/ban");
        if (cursor.HasValue)
            request.AddQueryParameter("cursor", cursor.ToString());
        var response = await Client.ExecuteAsync<IEnumerable<IRestBan>>(request);

        if (response.StatusCode != HttpStatusCode.OK)
            FailedRequest(response);

        return response.Data.Select(x => new Ban
        {
            BanID = x.Id.ToString(),
            BannedBy = x.BannedBy?.CanonicalKey,
            BannedOn = x.BannedOn.DateTime,
            BanType = x.BanType,
            CKey = x.CKey?.CanonicalKey,
            Expires = x.Expires?.DateTime,
            UnbannedBy = x.UnbannedBy?.CanonicalKey,
            Reason = x.Reason,
            JobBans = x.JobBans?.Select(j => new JobBan
                {
                    Job = j.Job
                })
                .ToHashSet(),
            SourceNavigation = Source
        });
    }

    public async Task<IEnumerable<Ban>> GetBansBatchedAsync(int? cursor = null, IEnumerable<int> searchFor = null)
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
            if (!lastResponse.Any())
                break;
            lastRequested = int.Parse(lastResponse[^1].BanID);
            result.AddRange(lastResponse);
        } while (lastResponse.Any() &&
                 (searchFor == null || lastResponse.Any(x => searchFor.Contains(int.Parse(x.BanID)))));

        return result;
    }

    public void Configure(StandardProviderConfiguration config)
    {
        if (_configured)
            throw new Exception("Cannot re-configure standard exporter provider, already configured");
        _configured = true;
        _baseUrl = config.Url;
        Source = new BanSource { Name = config.Id, Display = config.Display, RoleplayLevel = config.RoleplayLevel };

        // Re-initialize client with new url
        InitializeClient();

        // Setup JSON for client
        var options = new JsonSerializerOptions();
        options.AddCentComOptions();
        options.AddDataObjectConverter<IRestBan, RestBan>();
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.Converters.Insert(0, new JsonStringEnumConverter());
        Client.UseSystemTextJson(options);
    }
}