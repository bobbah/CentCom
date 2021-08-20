using System;
using System.Globalization;
using System.Text.Json.Serialization;
using CentCom.Common.Extensions;
using CentCom.Common.Models;

namespace CentCom.Server.External.Raw
{
    public class TgRawBan : IRawBan
    {
        private string _bannedAt;
        private string _expirationTime;
        private string _unbannedAt;

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("expiration_time")]
        public string ExpirationTimeRaw
        {
            get => _expirationTime; set
            {
                _expirationTime = value;
                ExpirationTime = ParseTgDateTime(value);
            }
        }

        public DateTime? ExpirationTime { get; private set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("ckey")]
        public string CKey { get; set; }

        [JsonPropertyName("a_ckey")]
        public string AdminCKey { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("bantime")]
        public string BannedAtRaw
        {
            get => _bannedAt; set
            {
                _bannedAt = value;
                BannedAt = ParseTgDateTime(value).Value;
            }
        }

        public DateTime BannedAt { get; private set; }

        [JsonPropertyName("unbanned_datetime")]
        public string UnbannedAtRaw
        {
            get => _unbannedAt; set
            {
                _unbannedAt = value;
                UnbannedAt = ParseTgDateTime(value);
            }
        }

        public DateTime? UnbannedAt { get; private set; }

        [JsonPropertyName("unbanned_Ckey")]
        public string UnbannedBy { get; set; }

        public BanType GetBanType() => Role.ToLower() == "server" ? BanType.Server : BanType.Job;

        public Ban AsBan(BanSource source)
        {
            var toReturn = new Ban()
            {
                BanID = Id.ToString(),
                BannedBy = AdminCKey,
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

        private static DateTime? ParseTgDateTime(string value)
        {
            if (DateTime.TryParse(value, 
                CultureInfo.InvariantCulture.DateTimeFormat,
                DateTimeStyles.AllowWhiteSpaces,
                out var expiration))
            {
                return DateTime.SpecifyKind(expiration, DateTimeKind.Utc);
            }

            return null;
        }
    }
}
