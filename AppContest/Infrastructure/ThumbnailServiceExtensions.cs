using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppContest.Infrastructure
{
    public static class ThumbnailServiceExtensions
    {
        public static string Thumbnail(this string url)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(url));
            sha1.Clear();
            return "https://storageaccountscree98ae.blob.core.windows.net/screenshots/" + BitConverter.ToString(hash).ToLower().Replace("-", "") + ".png";
        }
    }
}
