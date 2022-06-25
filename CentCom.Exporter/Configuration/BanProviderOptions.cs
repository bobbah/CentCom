using System;

namespace CentCom.Exporter.Configuration;

/// <summary>
/// The set of options available to configure on a standardized ban provider
/// </summary>
public class BanProviderOptions
{
    /// <summary>
    /// Enum controlling what job bans are included
    /// </summary>
    public BanInclusionOption JobBans { get; set; }

    /// <summary>
    /// Enum controlling what server bans are included
    /// </summary>
    public BanInclusionOption ServerBans { get; set; }

    /// <summary>
    /// If present dictates that only bans after this date will be provided
    /// </summary>
    public DateTimeOffset? AfterDate { get; set; }

    /// <summary>
    /// If present dictates that only bans after this ID will be provided
    /// </summary>
    public int? AfterId { get; set; }

    /// <summary>
    /// The limit of bans per page / query
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Controls if timestamps provided from the database are ASSUMED to be UTC
    /// </summary>
    /// /// <remarks>
    /// Note that no conversion of the timestamps will take place, they are ASSUMED to be in the correct timezone
    /// </remarks>
    public bool UseLocalTimezone { get; set; }

    /// <summary>
    /// Controls what UTC offset is used when UseLocalTimezone is true, if null then the local UTC offset is used
    /// </summary>
    /// <remarks>
    /// Note that no conversion of the timestamps will take place, they are ASSUMED to be in the correct timezone
    /// </remarks>
    public TimeSpan? UtcOffset { get; set; }
}