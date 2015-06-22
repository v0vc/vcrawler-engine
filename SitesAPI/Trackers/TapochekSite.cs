using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Interfaces.API;
using Interfaces.Models;
using Interfaces.POCO;

namespace SitesAPI.Trackers
{
    public class TapochekSite :ITapochekSite
    {
        

        public async Task<CookieCollection> GetUserCookieNetAsync(ICred cred)
        {
            if (string.IsNullOrEmpty(cred.Login) || string.IsNullOrEmpty(cred.Pass))
                throw new Exception("Please, set login and password");

            var hostUrl = string.Format("http://{0}", cred.Site);
            var loginUrl = string.Format("{0}/login.php", hostUrl);
            var indexUrl = string.Format("{0}/index.php", hostUrl);

            var req = (HttpWebRequest)WebRequest.Create(loginUrl);
            req.Method = WebRequestMethods.Http.Post;
            req.Host = cred.Site;
            req.KeepAlive = true;
            var postData = string.Format("login_username={0}&login_password={1}&login=%C2%F5%EE%E4", Uri.EscapeDataString(cred.Login), Uri.EscapeDataString(cred.Pass));
            var data = Encoding.ASCII.GetBytes(postData);
            req.ContentLength = data.Length;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Headers.Add("Cache-Control", "max-age=0");
            req.Headers.Add("Origin", hostUrl);
            req.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            req.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
            req.Headers.Add("DNT", "1");
            req.Referer = indexUrl;

            using (var stream = await req.GetRequestStreamAsync())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }

            var resp = (HttpWebResponse)(await req.GetResponseAsync());

            return resp.Cookies;
        }

        public Task<CookieCollection> GetUserCookieDbAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<IVideoItemPOCO>> GetUserItemsAsync(string userID, int maxResult)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetUserCountItemsAsync(string userID)
        {
            throw new NotImplementedException();
        }

        public Task<IVideoItemPOCO> GetVideoItemNetAsync(string videoid)
        {
            throw new NotImplementedException();
        }
    }
}
