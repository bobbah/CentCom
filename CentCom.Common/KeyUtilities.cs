using System;
using System.Text.RegularExpressions;

namespace CentCom.Common;

/// <summary>
/// Contains utilities for interacting with BYOND keys
/// </summary>
public static class KeyUtilities
{
    private static readonly Regex KeyReplacePattern = new Regex(@"[^a-z0-9]", RegexOptions.Compiled);

    /// <summary>
    /// Sanitizes a BYOND key into a valid ckey
    /// </summary>
    /// <param name="raw">The raw key to be sanitized</param>
    /// <returns>A sanitized BYOND canonical key</returns>
    /// <exception cref="ArgumentNullException">Null key provided</exception>
    public static string GetCanonicalKey(string raw)
    {
        if (raw == null)
        {
            throw new ArgumentNullException(nameof(raw));
        }

        return KeyReplacePattern.Replace(raw.ToLower(), "");
    }
}