using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentCom.Common.Abstract;
using CentCom.Common.Models;
using CentCom.Common.Models.Rest;
using CentCom.Exporter.Configuration;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace CentCom.Exporter.Data.Providers
{
    public class TgBanProvider : IBanProvider
    {
        private readonly string _connStr;
        private readonly List<IRestBan> _rawData;

        public TgBanProvider(IConfiguration config)
        {
            _connStr = config.GetConnectionString("provider");
            _rawData = new List<IRestBan>();
        }

        public async Task<IEnumerable<IRestBan>> GetBansAsync(int? cursor, BanProviderOptions options)
        {
            var result = new List<IRestBan>();
            while (result.Count < options.Limit)
            {
                var batchSize = await FetchMore(_rawData.Count > 0 ? _rawData[^1].Id : cursor, options);

                // Check for state not changing any further, indicates end of data
                if (batchSize == 0)
                {
                    result = FilterRaw(true, options).ToList();
                    break;
                }

                // Determine if we have reached the end of the dataset
                var belowLimit = batchSize < options.Limit;
                
                // Update result set
                result = FilterRaw(belowLimit, options).ToList();
            }

            // Take up to the limit of retrieved records
            return result.Take(Math.Min(result.Count, options.Limit));
        }

        private async Task<int> FetchMore(int? cursor, BanProviderOptions options)
        {
            const string query = @"
				SELECT
					b.id,
					b.bantime AS BannedAt,
					b.role,
					b.expiration_time AS Expiration,
					b.reason,
					b.ckey,
					b.a_ckey AS BannedBy,
					b.unbanned_datetime AS UnbannedAt,
					b.unbanned_ckey AS UnbannedBy
				FROM
					ban b
				WHERE
					(@cursor IS NULL OR b.id < @cursor)
					AND (@afterDate IS NULL OR b.bantime > @afterDate)
					AND (@afterId IS NULL OR b.id > @afterId)
					AND (@includeJobBans OR b.role = 'Server')
					AND (@includeServerBans OR b.role <> 'Server')
				ORDER BY b.id DESC
				LIMIT @limit";

            await using var conn = GetConnection();
            var rawBans = await conn.QueryAsync<ExportedBan>(query, new
                {
                    cursor,
                    options.Limit,
                    options.AfterDate,
                    options.AfterId,
                    includeJobBans = options.JobBans != BanInclusionOption.None,
                    includeServerBans = options.ServerBans != BanInclusionOption.None
                });

            // Specify timestamps UTC where appropriate
            if (!options.UseLocalTimezone)
            {
                foreach (var ban in rawBans)
                {
                    ban.BannedAt = new DateTimeOffset(ban.BannedAt.Ticks, options.UtcOffset ?? TimeSpan.Zero);
                    if (ban.Expiration.HasValue)
                        ban.Expiration = new DateTimeOffset(ban.Expiration.Value.Ticks, options.UtcOffset ?? TimeSpan.Zero);
                    if (ban.Unbanned.HasValue)
                        ban.Unbanned = new DateTimeOffset(ban.Unbanned.Value.Ticks, options.UtcOffset ?? TimeSpan.Zero);
                }
            }

            _rawData.AddRange(rawBans.Select(x => (RestBan)x));
            return rawBans?.Count() ?? 0;
        }

        private IEnumerable<IRestBan> FilterRaw(bool belowLimit, BanProviderOptions options)
        {
            // Filter based on allowed ban types
            var rawBans = _rawData.Where(x => JobBanFilter(x, options) || ServerBanFilter(x, options));

            // Cluster bans to coalesce job bans as appropriate
            var clustered = BanClusterer.ClusterBans(rawBans);

            // Always remove the last ban if it is a jobban and we aren't at the limit of data, as it could be a partial result.
            if (clustered.Any() && clustered.First().BanType == BanType.Job && !belowLimit)
            {
                clustered = clustered.SkipLast(1);
            }

            return clustered;
        }

        private static bool JobBanFilter(IRestBan ban, BanProviderOptions options)
            => ban.BanType switch
            {
                BanType.Server => false,
                BanType.Job => options.JobBans switch
                {
                    BanInclusionOption.None => false,
                    BanInclusionOption.Temporary => ban.Expires.HasValue,
                    BanInclusionOption.Permanent => !ban.Expires.HasValue,
                    BanInclusionOption.All => true,
                    _ => throw new ArgumentOutOfRangeException()
                },
                _ => throw new ArgumentOutOfRangeException()
            };

        private static bool ServerBanFilter(IRestBan ban, BanProviderOptions options)
            => ban.BanType switch
            {
                BanType.Job => false,
                BanType.Server => options.ServerBans switch
                {
                    BanInclusionOption.None => false,
                    BanInclusionOption.Temporary => ban.Expires.HasValue,
                    BanInclusionOption.Permanent => !ban.Expires.HasValue,
                    BanInclusionOption.All => true,
                    _ => throw new ArgumentOutOfRangeException()
                },
                _ => throw new ArgumentOutOfRangeException()
            };

        private MySqlConnection GetConnection() => new MySqlConnection(_connStr);
    }
}