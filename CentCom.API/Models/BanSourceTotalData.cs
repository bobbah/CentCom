namespace CentCom.API.Models;

/// <summary>
/// Aggregated ban count for a single source.
/// </summary>
public class BanSourceTotalData
{
    public int SourceID { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public int TotalBans { get; set; }
}
