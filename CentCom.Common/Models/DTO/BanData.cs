using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;

namespace CentCom.Common.Models.DTO;

/// <summary>
/// DTO for all data that a ban contains
/// </summary>
public class BanData
{
    /// <summary>
    /// Internal CentCom DB ID
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Internal CentCom DB ID for the ban source
    /// </summary>
    public int SourceID { get; set; }

    /// <summary>
    /// The textual name of the ban source
    /// </summary>
    public string SourceName { get; set; }

    /// <summary>
    /// The roleplay level of the ban source
    /// </summary>
    public RoleplayLevel SourceRoleplayLevel { get; set; }

    /// <summary>
    /// The type of ban
    /// </summary>
    public BanType Type { get; set; }

    /// <summary>
    /// The CKey of the banned user
    /// </summary>
    public string CKey { get; set; }

    /// <summary>
    /// The DateTime at which the user was banned
    /// </summary>
    public DateTime BannedOn { get; set; }

    /// <summary>
    /// The user who created the ban
    /// </summary>
    public string BannedBy { get; set; }

    /// <summary>
    /// The reason for the ban
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// The expiration, if present, of the ban
    /// </summary>
    public DateTime? Expires { get; set; }

    /// <summary>
    /// The user, if present, who removed the ban
    /// </summary>
    public string UnbannedBy { get; set; }

    /// <summary>
    /// The ban ID, if present, as provided by the source
    /// </summary>
    public string BanID { get; set; }

    /// <summary>
    /// The jobs, if present, that the user is banned from
    /// </summary>
    public List<string> Jobs { get; set; }

    /// <summary>
    /// Any additional attributes added to this ban by CentCom
    /// </summary>
    public List<string> BanAttributes { get; set; }

    /// <summary>
    /// If the ban is currently active, as predicted by CentCom
    /// </summary>
    public bool Active => (!Expires.HasValue || Expires.Value > DateTime.UtcNow) && UnbannedBy == null;

    /// <summary>
    /// Generates a BanData DTO from a database Ban.
    /// </summary>
    /// <param name="ban">The object to copy data from</param>
    /// <returns>A BanData DTO</returns>
    public static BanData FromBan(Ban ban)
    {
        return new BanData
        {
            ID = ban.Id,
            SourceID = ban.Source,
            SourceName = ban.SourceNavigation.Display,
            SourceRoleplayLevel = ban.SourceNavigation.RoleplayLevel,
            Type = ban.BanType,
            CKey = ban.CKey,
            BannedOn = ban.BannedOn,
            BannedBy = ban.BannedBy,
            Reason = ban.Reason,
            Expires = ban.Expires,
            UnbannedBy = ban.UnbannedBy,
            BanID = ban.BanID,
            Jobs = (ban.JobBans?.Count > 0) ? ban.JobBans.Select(x => x.Job).ToList() : null,
            BanAttributes = ban.BanAttributes.GetFlagCount() != 0
                ? ban.BanAttributes.GetFlags().Select(x => x.ToString()).ToList() : null
        };
    }
}