using System;
using System.Security.Cryptography;

namespace Ccf.Ck.Libs.Web.Bundling.Utils
{
    internal static class GeneralUtility
    {
        internal static string GenerateETag(byte[] data)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] md5hash = md5.ComputeHash(data);
                string hex = Convert.ToBase64String(md5hash);
                return hex.TrimEnd('=');
            }
        }
    }
}
