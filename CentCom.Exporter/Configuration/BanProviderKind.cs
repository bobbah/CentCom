namespace CentCom.Exporter.Configuration;

/// <summary>
/// The set of kinds of ban providers, used for configuration
/// </summary>
public enum BanProviderKind
{
    /// <summary>
    /// Specifies using a <see cref="CentCom.Exporter.Data.Providers.TgBanProvider" />
    /// </summary>
    Tgstation,
    /// <summary>
    /// Specifies using a <see cref="CentCom.Exporter.Data.Providers.ParadiseBanProvider" />
    /// </summary>
    ParadiseSS13,
}