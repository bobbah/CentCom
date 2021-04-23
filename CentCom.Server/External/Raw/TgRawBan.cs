using CentCom.Common.Extensions;
using CentCom.Common.Models;
using System;
using System.Text.Json.Serialization;

namespace CentCom.Server.External.Raw
{
    public enum TgBanStatus
    {
        Active,
        Expired,
        Unbanned
    }

    public class TgRawBan : IRawBan
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("status")]
        public TgBanStatus Status { get; set; }
        [JsonPropertyName("expiration_time")]
        public DateTime ExpirationTime { get; set; }
        [JsonPropertyName("expired")]
        public bool IsExpired { get; set; }
        [JsonPropertyName("role")]
        public string Role { get; set; }
        [JsonPropertyName("length")]
        public string Length { get; set; }
        [JsonPropertyName("ckey")]
        public string CKey { get; set; }
        [JsonPropertyName("a_ckey")]
        public string AdminCkey { get; set; }
        [JsonPropertyName("reason")]
        public string Reason { get; set; }
        [JsonPropertyName("bantime")]
        public DateTime BannedAt { get; set; }
        [JsonPropertyName("round_id")]
        public int Round { get; set; }
        [JsonPropertyName("server_port")]
        public int ServerPort { get; set; }
        [JsonPropertyName("unbanned_datetime")]
        public DateTime UnbannedAt { get; set; }
        [JsonPropertyName("unbanned_Ckey")]
        public string UnbannedBy { get; set; }
        [JsonPropertyName("unbanned")]
        public bool IsUnbanned { get; set; }
        [JsonPropertyName("applies_to_admins")]
        public bool AppliesToAdmins { get; set; }

        public BanType GetBanType() => Role.ToLower() == "server" ? BanType.Server : BanType.Job;

        public Ban AsBan(BanSource source)
        {
            var toReturn = new Ban()
            {
                BanID = Id.ToString(),
                BannedBy = AdminCkey,
                BannedOn = BannedAt,
                BanType = GetBanType(),
                CKey = CKey,
                UnbannedBy = UnbannedBy,
                Reason = Reason,
                Expires = ExpirationTime,
                SourceNavigation = source
            };

            if (toReturn.BanType == BanType.Job)
            {
                toReturn.AddJob(Role);
            }

            return toReturn;
        }
    }
}
