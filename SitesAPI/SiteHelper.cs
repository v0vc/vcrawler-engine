using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SitesAPI
{
    public static class SiteHelper
    {
        public static async Task<byte[]> GetStreamFromUrl(string url)
        {
            byte[] imageData;

            try
            {
                using (var wc = new WebClient())
                {
                    imageData = await wc.DownloadDataTaskAsync(url);
                }
            }
            catch
            {
                var b = Assembly.GetExecutingAssembly().GetManifestResourceStream("SitesAPI.Images.err404.png");
                return ReadFully(b);
            }

            return imageData;
        }

        public static byte[] ReadFully(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static async Task<string> DownloadStringAsync(Uri uri)
        {
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                try
                {
                    return await client.DownloadStringTaskAsync(uri);
                }
                catch (Exception ex)
                {
                    throw new Exception("Download Error: " + ex.Message);
                }
            }
        }

        public static async Task<string> DownloadStringWithCookieAsync(Uri uri, CookieCollection cookie)
        {
            var cc = new CookieContainer();
            cc.Add(cookie);
            using (var wc = new WebClientEx(cc))
            {
                var task = wc.DownloadStringTaskAsync(uri);
                task.Wait();
                return await task;
            }
        }

        public static string DownloadStringWithCookie(string link, CookieCollection cookie)
        {
            var cc = new CookieContainer();
            cc.Add(cookie);
            using (var wc = new WebClientEx(cc))
            {
                return wc.DownloadString(link);
            }
        }

        public static async Task<string> DownloadStringAsyncOld(Uri uri, int timeOut = 60000)
        {
            string res = null;

            var cancelledOrError = false;

            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.DownloadStringCompleted += (sender, e) =>
                {
                    if (e.Error != null || e.Cancelled)
                    {
                        cancelledOrError = true;
                    }
                    else
                    {
                        res = e.Result;
                    }
                };
                client.DownloadStringAsync(uri);
                var n = DateTime.Now;
                while (res == null && !cancelledOrError && DateTime.Now.Subtract(n).TotalMilliseconds < timeOut)
                {
                    await Task.Delay(100); // wait for respsonse
                }
            }
            if (res == null)
                throw new Exception("Download Error: " + uri.Segments.Last());

            return res;
        }
    }
}
