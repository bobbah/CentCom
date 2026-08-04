using System.ComponentModel;
using CentCom.Common;
using CentCom.Common.Models;
using CentCom.Common.Models.DTO;
using CentCom.MCP.Services;
using ModelContextProtocol.Server;

namespace CentCom.MCP.Tools;

[McpServerToolType]
internal class BanTools
{
    [McpServerTool]
    [Description(
        "Retrieve a known user's bans from the CentCom database. Make one call with only `key` to get the complete ban history across all sources, including both active and inactive server and job bans. Add an optional filter only when the user explicitly requests that restriction; do not send its default value. Do not make separate filtered calls to assemble the complete history. `key` must be exact; use `search_users` first when it is unknown.")]
    public async Task<IEnumerable<BanData>> GetBansForKey(
        IBanDataService bans,
        [Description("Required exact BYOND username or canonical key (ckey). Partial matching is not supported; use `search_users` when the exact key is unknown.")]
        string key,
        [Description(
            "Filter to one CentCom ban source only when the user requests a specific server. Omit this parameter to include every source; do not pass 0 for that behavior. Use `get_ban_sources` only if a requested server's ID is unknown.")]
        int source = 0,
        [Description(
            "Set to true only when the user asks for active bans exclusively. Omit this parameter to include both active and inactive bans in one response; never make additional true/false requests to obtain all bans.")]
        bool onlyActive = false,
        [Description(
            "Inclusive UTC lower bound for ban creation time in ISO 8601 format (for example, 2024-01-15T00:00:00Z). Omit unless the user requests a date restriction; never pass the default minimum date.")]
        DateTime createdAfter = default,
        [Description(
            "Inclusive UTC upper bound for ban creation time in ISO 8601 format (for example, 2024-12-31T23:59:59.9999999Z). Omit unless the user requests a date restriction; never pass the default minimum date.")]
        DateTime createdBefore = default,
        [Description(
            "Filter by ban type only when requested: `server` for server/game bans or `job` for role bans. Omit this parameter to include both types; do not pass an empty string.")]
        string banType = ""
    )
    {
        BanType? parsedBanType = banType.ToLowerInvariant() switch
        {
            "" => null,
            "server" => BanType.Server,
            "job" => BanType.Job,
            _ => throw new ArgumentException($"Invalid ban type '{banType}'. Valid values are 'server' or 'job'.")
        };

        return await bans.GetBansForKeyAsync(
            key,
            source == 0 ? null : source,
            onlyActive,
            createdAfter == default ? null : createdAfter,
            createdBefore == default ? null : createdBefore,
            parsedBanType);
    }

    [McpServerTool]
    [Description(
        "List every ban source (game server) known to CentCom, including its ID, display name, and roleplay level. Call this only when the user asks to inspect servers or requests a source-specific ban search whose source ID is unknown. It is not needed for a complete ban lookup because omitting `source` from `get_bans_for_key` already includes every source.")]
    public async Task<IEnumerable<BanSourceData>> GetBanSources(IBanDataService bans) =>
        await bans.GetAllBanSourcesAsync();

    [McpServerTool]
    [Description(
        "Find users with recorded bans by partial BYOND username/ckey. Use this to discover an exact key before calling `get_bans_for_key`. Only users with bans are returned, with ban counts and most recent ban time.")]
    public async Task<IEnumerable<KeySummary>> SearchUsers(
        IBanDataService bans,
        [Description(
            "Required partial BYOND username/ckey. Supply at least 3 characters after canonicalization (lowercase alphanumeric only); matching is case-insensitive.")]
        string query,
        [Description(
            "Maximum number of matches, from 1 to 50. Omit this parameter for the standard 25 results; provide it only when the user requests a different result count.")]
        int limit = 25
    )
    {
        var ckey = KeyUtilities.GetCanonicalKey(query);
        if (string.IsNullOrWhiteSpace(ckey) || ckey.Length < 3)
        {
            throw new ArgumentException(
                "Search query must be at least 3 characters long after canonicalization (lowercase alphanumeric).");
        }

        if (limit is < 1 or > 50)
            throw new ArgumentOutOfRangeException(nameof(limit), "Search result limit must be between 1 and 50.");

        return await bans.SearchUsersAsync(query, limit);
    }
}