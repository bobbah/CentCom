using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CentCom.Common.Models;

namespace CentCom.Common.Abstract;

/// <summary>
/// A standardized representation of a ban to be serialized
/// </summary>
public interface IRestBan
{
    /// <summary>
    /// The ID of the ban in the source database
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The type of ban
    /// </summary>
    public BanType BanType { get; }

    /// <summary>
    /// The banned player's ckey
    /// </summary>
    public ICKey CKey { get; }

    /// <summary>
    /// The time at which the player was banned
    /// </summary>
    public DateTimeOffset BannedOn { get; }

    /// <summary>
    /// The player who placed the ban
    /// </summary>
    public ICKey BannedBy { get; }

    /// <summary>
    /// The reason for the ban
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// When the ban expires, permanent if absent
    /// </summary>
    public DateTimeOffset? Expires { get; }

    /// <summary>
    /// The player who lifted the ban
    /// </summary>
    public ICKey UnbannedBy { get; }

    /// <summary>
    /// The list of job bans, if present
    /// </summary>
    public IReadOnlyList<IRestJobBan> JobBans { get; }

    /// <summary>
    /// The optional Round ID of the ban, if present
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? RoundId { get; }
}