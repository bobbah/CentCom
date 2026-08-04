using CentCom.Common.Models;
using CentCom.Common.Models.DTO;

namespace CentCom.MCP.Services;

public interface IBanDataService
{
    Task<IEnumerable<BanData>> GetBansForKeyAsync(
        string key,
        int? source,
        bool onlyActive = false,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        BanType? banType = null);

    Task<IEnumerable<BanSourceData>> GetAllBanSourcesAsync();

    Task<IEnumerable<KeySummary>> SearchUsersAsync(string query, int limit = 25);
}