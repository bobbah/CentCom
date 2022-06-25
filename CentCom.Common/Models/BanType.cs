namespace CentCom.Common.Models;

/// <summary>
/// Denotes the function of a ban from the ban source
/// </summary>
public enum BanType : uint
{
    /// <summary>
    /// Server bans, generally indicate a ban across the source's servers.
    /// </summary>
    Server,
    /// <summary>
    /// A job ban, may include an individual or range of jobs.
    /// </summary>
    Job
}