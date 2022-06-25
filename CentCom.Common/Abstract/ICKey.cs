namespace CentCom.Common.Abstract;

/// <summary>
/// Representation of a BYOND canonical key, or ckey
/// </summary>
public interface ICKey
{
    /// <summary>
    /// The canonical key of the user, a sanitized string of a BYOND user's username (key)
    /// </summary>
    public string CanonicalKey { get; }
}