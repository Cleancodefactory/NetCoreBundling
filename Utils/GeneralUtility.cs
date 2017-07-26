using System;
using System.Security.Cryptography;

namespace ccf.CoreKraft.Web.Bundling.Utils
{
    internal static class GeneralUtility
    {
        internal static string GenerateETag(byte[] data)
        {
            string ret = string.Empty;

            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                string hex = BitConverter.ToString(hash);
                ret = hex.Replace("-", "");
            }
            return ret;
        }
    }
}
