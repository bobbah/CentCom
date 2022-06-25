using CentCom.Common.Models;

namespace CentCom.API.Models;

/// <summary>
/// DTO for ban sources
/// </summary>
public class BanSourceData
{
    /// <summary>
    /// Internal CentCom DB ID
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The roleplay level of this ban source
    /// </summary>
    public RoleplayLevel RoleplayLevel { get; set; }

    /// <summary>
    /// Generates a DTO from a database BanSource
    /// </summary>
    /// <param name="source">The object to copy data from</param>
    /// <returns>A BanSource DTO</returns>
    public static BanSourceData FromBanSource(BanSource source)
    {
        return new BanSourceData
        {
            ID = source.Id,
            Name = source.Display,
            RoleplayLevel = source.RoleplayLevel
        };
    }
}