using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CentCom.Common
{
    public static class KeyUtilities
    {
        private static Regex _keyReplacePattern = new Regex(@"[^a-z0-9]");

        public static string GetCanonicalKey(string raw)
        {
            if (raw == null)
            {
                throw new ArgumentNullException(nameof(raw));
            }

            return _keyReplacePattern.Replace(raw.ToLower(), "");
        }
    }
}
