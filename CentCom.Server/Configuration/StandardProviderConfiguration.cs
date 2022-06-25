using CentCom.Common.Models;

namespace CentCom.Server.Configuration;

/// <summary>
/// Configuration for a standard ban provider source
/// </summary>
public class StandardProviderConfiguration
{
    /// <summary>
    /// The ID of the ban source
    /// </summary>
    /// <example>tgstation</example>
    public string Id { get; set; }

    /// <summary>
    /// The display name of the ban source
    /// </summary>
    /// <example>/tg/station</example>
    public string Display { get; set; }

    /// <summary>
    /// The roleplay level of the ban source
    /// </summary>
    public RoleplayLevel RoleplayLevel { get; set; }

    /// <summary>
    /// The URL of the ban exporter
    /// </summary>
    /// <remarks>
    /// Do not include the /api/ban path, or /api/, just the base URL. Include a trailing slash.
    /// </remarks>
    /// <example>
    /// https://localhost:6658/
    /// </example>
    public string Url { get; set; }
}