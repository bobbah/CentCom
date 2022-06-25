using System;

namespace CentCom.Common.Models;

public class NotifiedFailure
{
    public long Id { get; set; }
    public long CheckHistoryId { get; set; }
    public virtual CheckHistory CheckHistory { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}