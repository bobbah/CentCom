using System;

namespace CentCom.Common.Models;

/// <summary>
/// Additional attributes necessitated or used by individual ban sources
/// which are not common but are requested by those ban sources.
/// </summary>
[Flags]
public enum BanAttribute
{
    None = 1 << 0,
    /// <summary>
    /// Global bans on BeeStation, noting a ban applies across all of their servers.
    /// </summary>
    BeeStationGlobal = 1 << 1
}