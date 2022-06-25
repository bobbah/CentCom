using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using CentCom.Common.Extensions;
using CentCom.Common.Models;
using Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace CentCom.Server.Services;

public class FulpBanService : RestBanService
{
    private const int RecordsPerPage = 50;
    private static readonly BanSource BanSource = new BanSource { Name = "fulp" };

    public FulpBanService(ILogger<FulpBanService> logger, IConfiguration config) : base(logger)
    {
        if (config.GetSection("sourceConfig").GetValue<bool>("allowFulpExpiredSSL"))
        {
            Client.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyError) => true;
        }
    }

    protected override string BaseUrl => "https://api.fulp.gg/";

    public async Task<IEnumerable<Ban>> GetBansAsync(int page = 1)
    {
        var request = new RestRequest($"bans/{RecordsPerPage}/{page}");
        var response = await Client.ExecuteAsync(request);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            FailedRequest(response);
        }

        var toReturn = new List<Ban>();
        var content = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(response.Content);
        foreach (var ban in content["value"].GetProperty("bans").EnumerateArray())
        {
            // Need to get both the expiration as well as the unbanned time as they can differ
            DateTime? expiration = null;
            var unbannedTime = ban.GetProperty("unbannedTime").GetString();
            var expireTime = ban.GetProperty("banExpireTime").GetString();
            if (unbannedTime != null)
                expiration = DateTime.Parse(unbannedTime);
            if (expireTime != null)
                expiration ??= DateTime.Parse(expireTime);

            // Get ban
            var toAdd = new Ban
            {
                BannedOn = DateTime.Parse(ban.GetProperty("banApplyTime").GetString()),
                BannedBy = ban.GetProperty("adminCkey").GetString(),
                BanType = ban.GetProperty("role")[0].GetString().ToLower() == "server"
                    ? BanType.Server
                    : BanType.Job,
                Expires = expiration,
                CKey = ban.GetProperty("bannedCkey").GetString(),
                Reason = ban.GetProperty("reason").GetString(),
                SourceNavigation = BanSource
            };

            // Add jobs if relevant
            if (toAdd.BanType == BanType.Job)
            {
                toAdd.AddJobRange(ban.GetProperty("role").EnumerateArray().Select(x => x.GetString()));
            }

            // Specify UTC
            toAdd.BannedOn = DateTime.SpecifyKind(toAdd.BannedOn, DateTimeKind.Utc);
            if (toAdd.Expires.HasValue)
            {
                toAdd.Expires = DateTime.SpecifyKind(toAdd.Expires.Value, DateTimeKind.Utc);
            }

            toReturn.Add(toAdd);
        }

        return toReturn;
    }

    public async Task<IEnumerable<Ban>> GetBansBatchedAsync(int startPage = 1, int pages = -1)
    {
        var maxPages = await GetNumberOfPagesAsync();
        var range = Enumerable.Range(startPage, pages != -1 ? Math.Min(startPage + pages, maxPages) : maxPages);
        var toReturn = new ConcurrentBag<Ban>();
        await range.AsyncParallelForEach(async page =>
        {
            foreach (var b in await GetBansAsync(page))
            {
                toReturn.Add(b);
            }
        }, 6);
        return toReturn;
    }

    public async Task<int> GetNumberOfPagesAsync()
    {
        var request = new RestRequest($"bans/{RecordsPerPage}/1");
        var result = await Client.ExecuteAsync(request);

        if (result.StatusCode != HttpStatusCode.OK)
        {
            FailedRequest(result);
        }

        var content = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result.Content);
        if (content["value"].TryGetProperty("lastPage", out var lastpage))
        {
            return lastpage.GetInt32();
        }

        throw new Exception("Failed to find the last page number in the response from Fulpstation's API");
    }
}