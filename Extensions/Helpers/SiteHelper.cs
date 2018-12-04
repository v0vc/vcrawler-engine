// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Extensions.Helpers
{
    public static class SiteHelper
    {
        #region Static Methods

        public static bool CheckForInternetConnection(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (Stream stream = client.OpenRead(url))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<string> DownloadStringAsync(Uri uri)
        {
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                try
                {
                    return await client.DownloadStringTaskAsync(uri).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new Exception("Download Error: " + ex.Message);
                }
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
                DateTime n = DateTime.Now;
                while (res == null && !cancelledOrError && DateTime.Now.Subtract(n).TotalMilliseconds < timeOut)
                {
                    await Task.Delay(100).ConfigureAwait(false); // wait for respsonse
                }
            }
            if (res == null)
            {
                throw new Exception("Download Error: " + uri.Segments.Last());
            }

            return res;
        }

        public static string DownloadStringWithCookie(string link, CookieContainer cookie)
        {
            using (var wc = new WebClientEx(cookie))
            {
                return wc.DownloadString(link);
            }
        }

        public static async Task<string> DownloadStringWithCookieAsync(Uri uri, CookieContainer cookie)
        {
            using (var wc = new WebClientEx(cookie))
            {
                Task<string> task = wc.DownloadStringTaskAsync(uri);
                task.Wait();
                return await task.ConfigureAwait(false);
            }
        }

        public static async Task<byte[]> GetStreamFromUrl(string url)
        {
            byte[] imageData;

            try
            {
                using (var wc = new WebClient())
                {
                    imageData = await wc.DownloadDataTaskAsync(url).ConfigureAwait(false);
                }
            }
            catch
            {
                Stream b = Assembly.GetExecutingAssembly().GetManifestResourceStream("Extensions.Images.err404.png");
                return StreamHelper.ReadFully(b);
            }

            return imageData;
        }

        public static bool IsUrlExist(string url)
        {
            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                if (request != null)
                {
                    request.Method = "HEAD";
                    var response = request.GetResponse() as HttpWebResponse;
                    if (response != null)
                    {
                        response.Close();
                        return response.StatusCode == HttpStatusCode.OK;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static async Task<string> PostMethod(string requestStr)
        {
            WebRequest request = WebRequest.Create(requestStr);
            request.Method = "POST";
            request.ContentLength = 0;
            try
            {
                using (WebResponse res = await request.GetResponseAsync())
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        if (stream == null)
                        {
                            return string.Empty;
                        }
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            return await reader.ReadToEndAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion
    }
}
