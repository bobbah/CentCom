using System;
using CentCom.Common.Models;
using CentCom.Common.Models.Byond;
using CentCom.Common.Models.Rest;

namespace CentCom.Exporter.Data.Ban;

/// <summary>
/// Standard exported ban from a IBanProvider
/// </summary>
/// <remarks>
/// This class may require modification should another IBanProvider implementation necessitate it
/// </remarks>
public class ExportedBan {
    /// <summary>
    /// The database ID of the ban
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The time at which the ban was placed
    /// </summary>
    public DateTimeOffset BannedAt { get; set; }

    /// <summary>
    /// The role (typically job) of which the ban applies to, 'Server' dictates that this was not a job ban.
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// The type of ban
    /// </summary>

    public BanType BanType => Role.Equals("server", StringComparison.InvariantCultureIgnoreCase)
        ? BanType.Server
        : BanType.Job;

    /// <summary>
    /// The expiration date of the ban, permanent if not present
    /// </summary>
    public DateTimeOffset? Expiration { get; set; }

    /// <summary>
    /// The reason of the ban
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// The banned player
    /// </summary>
    public CKey CKey { get; set; }

    /// <summary>
    /// The player who placed the ban
    /// </summary>
    public CKey BannedBy { get; set; }

    /// <summary>
    /// The time at which the ban was lifted
    /// </summary>
    public DateTimeOffset? Unbanned { get; set; }

    /// <summary>
    /// The player who lifted the ban
    /// </summary>
    public CKey UnbannedBy { get; set; }

    /// <summary>
    /// The Round ID of the ban, if any
    /// </summary>
    public int? RoundId { get; set; }

    public static implicit operator RestBan(ExportedBan ban) => new RestBan
    (
        ban.Id,
        ban.BanType,
        ban.CKey,
        ban.BannedAt,
        ban.BannedBy,
        ban.Reason,
        ban.Expiration,
        ban.UnbannedBy,
        ban.BanType == BanType.Job ? new[] { new RestJobBan(ban.Role) } : null,
        ban.RoundId
    );
}