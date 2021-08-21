using System;
using CentCom.Exporter.Data;

namespace CentCom.Exporter.Configuration
{
    public class BanProviderOptions
    {
        public BanInclusionOption JobBans { get; set; }
        public BanInclusionOption ServerBans { get; set; }
        public DateTimeOffset? AfterDate { get; set; }
        public int? AfterId { get; set; }
        public int Limit { get; set; }
        public bool UseLocalTimezone { get; set; }
        public TimeSpan? UtcOffset { get; set; }
    }
}