using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SitesAPI
{
    public static class SiteHelper
    {
        public static async Task<byte[]> GetStreamFromUrl(string url)
        {
            byte[] imageData;

            using (var wc = new WebClient())
            {
                imageData = await wc.DownloadDataTaskAsync(url);
            }

            return imageData;
        }
    }
}
