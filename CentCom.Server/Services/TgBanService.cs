using CentCom.Common.Extensions;
using CentCom.Common.Models;
using CentCom.Server.Exceptions;
using CentCom.Server.External;
using CentCom.Server.External.Raw;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentCom.Server.Services
{
    public class TgBanService
    {
        private readonly IRestClient _client;
        private readonly ILogger _logger;
        private const string BASE_URL = "https://tgstation13.org/";
        private readonly static BanSource _banSource = new BanSource { Name = "tgstation" };

        public TgBanService(ILogger<TgBanService> logger)
        {
            _logger = logger;
            _client = new RestClient(BASE_URL);
            _client.UseSystemTextJson(new System.Text.Json.JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private async Task<List<TgRawBan>> GetBansAsync(int? startingId = null)
        {
            var request = new RestRequest($"tgdb/publicbans.php", Method.GET, DataFormat.Json)
                .AddQueryParameter("format", "json");
            if (startingId.HasValue)
                request.AddQueryParameter("beforeid", startingId.ToString());
            var response = await _client.ExecuteAsync<TgApiResponse>(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var errorMessage = $"tgdb returned a non-200 HTTP response code: { response.StatusCode}, aborting parse.";
                _logger.LogError(errorMessage);
                throw new BanSourceUnavailableException(errorMessage);
            }

            return response.Data.Bans.ToList();
        }

        public async Task<IEnumerable<Ban>> GetBansBatchedAsync(int? startingId = null, IEnumerable<int> searchFor = null)
        {
            // Get bans, must use a sequential approach here due to the last ban
            // ID only being available once we finish a page
            var dirtyBans = new List<TgRawBan>();
            int? lastRequested = startingId;
            List<TgRawBan> lastResponse = null;
            do
            {
                lastResponse = await GetBansAsync(lastRequested);

                // If the last ban on the page is a job ban, get the next page to ensure we have the full ban
                if (lastResponse[^0].GetBanType() == BanType.Job)
                {
                    var nextPage = await GetBansAsync(lastResponse[^0].Id);
                    lastResponse.AddRange(nextPage.Where(x => x.CKey == lastResponse[^0].CKey && x.BannedAt == lastResponse[^0].BannedAt));
                }

                lastRequested = lastResponse.Min(x => x.Id);
                dirtyBans.AddRange(lastResponse);
            }
            while (lastResponse.Any() && (searchFor == null || lastResponse.Any(x => searchFor.Contains(x.Id))));

            // Flatten any jobbans
            var intermediateBans = dirtyBans.Select(x => x.AsBan(_banSource));
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
}
