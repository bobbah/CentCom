using System;

namespace CentCom.Common.Models;

public class CheckHistory
{
    public long Id { get; set; }
    public string Parser { get; set; }
    public DateTimeOffset Started { get; set; }
    public DateTimeOffset? CompletedDataFetch { get; set; }
    public DateTimeOffset? CompletedUpload { get; set; }
    public DateTimeOffset? Failed { get; set; }
    public int Added { get; set; }
    public int Updated { get; set; }
    public int Deleted { get; set; }
    public int Erroneous { get; set; }
    public bool Success { get; set; }
    public bool CompleteRefresh { get; set; }
    public string Exception { get; set; }
    public string ExceptionDetailed { get; set; }
    public string ResponseContent { get; set; }
    public virtual NotifiedFailure Notification { get; set; }
}