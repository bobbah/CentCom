using System.Collections.Generic;
using CentCom.Common.Abstract;

namespace CentCom.Exporter.Data.Clustering;

/// <summary>
/// Provides utilities for clustering bans such that job bans are correctly grouped into one ban
/// </summary>
public interface IBanClusterer {
    /// <summary>
    /// Clusters a collection of IRestBans to ensure that all job bans are grouped appropriately
    /// </summary>
    /// <param name="bans">The bans to cluster</param>
    /// <returns>The collection of bans with all job bans clustered appropriately</returns>
    public IEnumerable<IRestBan> ClusterBans(IEnumerable<IRestBan> bans);
}
