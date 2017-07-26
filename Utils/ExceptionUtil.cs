using System;

namespace ccf.CoreKraft.Web.Bundling.Utils
{
    internal static class ExceptionUtil
    {
        internal static bool IsPureWildcardSearchPattern(string searchPattern)
        {
            if (!string.IsNullOrEmpty(searchPattern))
            {
                string trimmed = searchPattern.Trim();
                if (string.Equals(trimmed, "*", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(trimmed, "*.*", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
