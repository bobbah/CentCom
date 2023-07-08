using CentCom.Common.Models;
using CentCom.Common.Models.Rest;

namespace CentCom.Exporter.Data.Ban;
public class ParadiseExportedBan : ExportedBan {
    public new BanType BanType => ((InternalBanType.Equals("PERMABAN") || InternalBanType.Equals("TEMPBAN")) ? BanType.Server : BanType.Job);

    /// <summary>
    /// The internal type for this ban.
    /// </summary>
    public string InternalBanType { get; set; }

    // If you dont override this, stuff breaks
    public static implicit operator RestBan(ParadiseExportedBan ban) => new RestBan
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
