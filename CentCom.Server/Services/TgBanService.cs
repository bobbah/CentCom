﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CentCom.Common.Extensions;
using CentCom.Common.Models;
using CentCom.Server.External;
using CentCom.Server.External.Raw;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Serializers.Json;

namespace CentCom.Server.Services;

public class TgBanService : RestBanService
{
    private static readonly BanSource BanSource = new BanSource { Name = "tgstation" };

    public TgBanService(ILogger<TgBanService> logger) : base(logger)
    {
        // Re-initialize to control JSON serialization behaviour
        InitializeClient(o =>o.UseSystemTextJson(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        }));
    }

    protected override string BaseUrl => "https://tgstation13.org/";

    private async Task<List<TgRawBan>> GetBansAsync(int? startingId = null)
    {
        var request = new RestRequest("tgdb/publicbans.php")
            .AddQueryParameter("format", "json");
        if (startingId.HasValue)
            request.AddQueryParameter("beforeid", startingId.ToString());
        var response = await Client.ExecuteAsync<TgApiResponse>(request);

        if (response.StatusCode != HttpStatusCode.OK)
            FailedRequest(response);

        return response.Data.Bans.ToList();
    }

    public async Task<IEnumerable<Ban>> GetBansBatchedAsync(int? startingId = null, IEnumerable<int> searchFor = null)
    {
        // Get bans, must use a sequential approach here due to the last ban
        // ID only being available once we finish a page
        var dirtyBans = new List<TgRawBan>();
        var lastRequested = startingId;
        List<TgRawBan> lastResponse;
        do
        {
            lastResponse = await GetBansAsync(lastRequested);
            if (!lastResponse.Any())
                break;

            // If the last ban on the page is a job ban, get the next page to ensure we have the full ban
            if (lastResponse[^1].GetBanType() == BanType.Job)
            {
                var nextPage = await GetBansAsync(lastResponse[^1].Id);
                lastResponse.AddRange(nextPage.Where(x => x.CKey == lastResponse[^1].CKey && x.BannedAt == lastResponse[^1].BannedAt));
            }

            lastRequested = lastResponse.Min(x => x.Id);
            dirtyBans.AddRange(lastResponse);
        }
        while (lastResponse.Any() && (searchFor == null || lastResponse.Any(x => searchFor.Contains(x.Id))));

        // Flatten any jobbans
        var intermediateBans = dirtyBans.Select(x => x.AsBan(BanSource));
        var cleanBans = intermediateBans.Where(x => x.BanType == BanType.Server).ToList();
        foreach (var group in intermediateBans.Where(x => x.BanType == BanType.Job).GroupBy(x => new { x.CKey, x.BannedOn }))
        {
            var firstBan = group.OrderBy(x => x.BanID).First();
            firstBan.AddJobRange(group.SelectMany(x => x.JobBans).Select(x => x.Job));
            cleanBans.Add(firstBan);
        }

        return cleanBans;
    }
}