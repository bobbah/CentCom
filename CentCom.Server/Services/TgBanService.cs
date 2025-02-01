using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CentCom.Common.Models;
using CentCom.Server.External;
using CentCom.Server.External.Raw;
using Microsoft.Extensions.Logging;

namespace CentCom.Server.Services;

public class TgBanService(HttpClient client, ILogger<TgBanService> logger) : HttpBanService(client, logger)
{
    protected override string BaseUrl => "https://statbus.space/";

    public async Task<List<TgRawBan>> GetBansAsync(int? page = null) =>
        (await GetAsync<TgApiResponse>($"bans/public/v1/{page}",
            new Dictionary<string, string>() { { "json", "true" } })).Data.ToList();

    public async Task<List<Ban>> GetBansBatchedAsync(BanSource source)
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

        return allBans.Select(x => x.AsBan(source)).DistinctBy(x => x.BanID).ToList();
    }
}