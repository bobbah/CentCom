using System.Collections.Generic;
using System.Threading.Tasks;
using CentCom.Common.Abstract;
using CentCom.Exporter.Configuration;

namespace CentCom.Exporter.Data.Providers;

/// <summary>
/// A standard implementation of a ban provider
/// </summary>
public interface IBanProvider
{
    /// <summary>
    /// Retrieves bans from the datasource up to the configured limit per page
    /// </summary>
    /// <param name="cursor">An optional cursor to retrieve the bans from, NOT INCLUSIVE</param>
    /// <param name="options">The options for the request</param>
    /// <returns>A collection of bans up to the configured limit of bans per page</returns>
    public Task<IEnumerable<IRestBan>> GetBansAsync(int? cursor, BanProviderOptions options);
}