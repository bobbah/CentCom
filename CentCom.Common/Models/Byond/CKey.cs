using CentCom.Common.Abstract;

namespace CentCom.Common.Models.Byond;

/// <summary>
/// A CKey constructed from a BYOND key
/// </summary>
/// <remarks>
/// Performs necessary sanitization to create a valid ckey
/// </remarks>
/// <param name="Key">The unsanitized BYOND key</param>
public record CKey(string Key) : ICKey
{
    /// <inheritdoc />
    public string CanonicalKey => KeyUtilities.GetCanonicalKey(Key);

    public static implicit operator CKey(string key) => new CKey(key);
}