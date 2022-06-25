namespace CentCom.Common.Abstract;

/// <summary>
/// A standardized representation of a job ban to be serialized
/// </summary>
public interface IRestJobBan
{
    /// <summary>
    /// The job from which the player was banned
    /// </summary>
    public string Job { get; }
}