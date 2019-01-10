using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace Ccf.Ck.Libs.Web.Bundling.Utils
{
    internal static class GeneralUtility
    {
        internal static string GenerateETag(byte[] data)
        {
            using (MD5 md5 = MD5.Create())
            {
                return WebEncoders.Base64UrlEncode(md5.ComputeHash(data));
            }
        }
    }
}
