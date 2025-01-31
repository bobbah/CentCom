using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }, 
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public TgBanService(ILogger<TgBanService> logger) : base(logger)
    {
        // Re-initialize to control JSON serialization behaviour
        InitializeClient(o => o.UseSystemTextJson(JsonOptions));
    }

    protected override string BaseUrl => "https://statbus.space/";

    public async Task<List<TgRawBan>> GetBansAsync(int? page = null)
    {
        var request = new RestRequest($"bans/public/v1/{page}")
            .AddQueryParameter("json", "true");
        var response = await Client.ExecuteAsync<TgApiResponse>(request);

        if (response.StatusCode != HttpStatusCode.OK)
            FailedRequest(response);

        return response.Data.Data.ToList();
    }

    public async Task<IEnumerable<Ban>> GetBansBatchedAsync()
    {
        var allBans = new List<TgRawBan>();
        var page = 1;
        while (true)
        {
            var bans = await GetBansAsync(page);
            if (bans.Count == 0)
                break;

            allBans.AddRange(bans);
            page++;
        }

        return allBans.Select(x => x.AsBan(BanSource)).DistinctBy(x => x.BanID);
    }
}