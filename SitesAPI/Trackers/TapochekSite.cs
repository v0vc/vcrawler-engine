using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Interfaces.API;
using Interfaces.Models;
using Interfaces.POCO;
using SitesAPI.POCO;

namespace SitesAPI.Trackers
{
    public class TapochekSite :ITapochekSite
    {
        private const string Site = "tapochek.net";
        private static readonly string HostUrl = string.Format("http://{0}", Site);
        private readonly string _loginUrl = string.Format("{0}/login.php", HostUrl);
        private readonly string _userUrl = string.Format("{0}/tracker.php?rid", HostUrl);
        private readonly string _searchUrl = string.Format("{0}/tracker.php?nm", HostUrl);
        private readonly string _topicUrl = string.Format("{0}/viewtopic.php?t", HostUrl);
        private readonly string _indexUrl = string.Format("{0}/index.php", HostUrl);

        public async Task<CookieCollection> GetCookieNetAsync(ICred cred)
        {
            if (string.IsNullOrEmpty(cred.Login) || string.IsNullOrEmpty(cred.Pass))
                throw new Exception("Please, set login and password");

            var cc = new CookieContainer();
            var req = (HttpWebRequest)WebRequest.Create(_loginUrl);
            req.CookieContainer = cc;
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
            req.Headers.Add("Origin", HostUrl);
            req.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            req.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
            req.Headers.Add("DNT", "1");
            req.Referer = _indexUrl;

            using (var stream = await req.GetRequestStreamAsync())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }

            var resp = (HttpWebResponse)(await req.GetResponseAsync());

            return resp.Cookies;
        }

        public async Task<List<IVideoItemPOCO>> GetChannelItemsAsync(IChannel channel, int maxresult)
        {
            var lst = new List<IVideoItemPOCO>();

            var zap = string.Format("{0}={1}", _userUrl, channel.ID);

            var page = await SiteHelper.DownloadStringWithCookieAsync(new Uri(zap), channel.ChannelCookies);

            var doc = new HtmlDocument();

            doc.LoadHtml(page);

            var links = doc.DocumentNode.Descendants("tr")
                .Where(
                    d =>
                        d.Attributes.Contains("class") &&
                        d.Attributes["class"].Value.Equals("tCenter")).ToList();

            foreach (HtmlNode node in links)
            {
                var vi = new VideoItemPOCO(node, Site);
                if (!string.IsNullOrEmpty(vi.ID))
                    lst.Add(vi);
            }

            if (maxresult == 0)
            {
                var searchlinks = GetAllSearchLinks(doc);

                foreach (string link in searchlinks)
                {
                    page = await SiteHelper.DownloadStringWithCookieAsync(new Uri(link), channel.ChannelCookies);

                    doc = new HtmlDocument();

                    doc.LoadHtml(page);

                    links = doc.DocumentNode.Descendants("tr")
                        .Where(
                            d =>
                                d.Attributes.Contains("class") &&
                                d.Attributes["class"].Value.Equals("tCenter")).ToList();

                    foreach (HtmlNode node in links)
                    {
                        var vi = new VideoItemPOCO(node, Site);
                        if (!string.IsNullOrEmpty(vi.ID))
                            lst.Add(vi);
                    }
                }
            }

            return lst;
        }

        public Task<IVideoItemPOCO> GetVideoItemNetAsync(string videoid)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<string> GetAllSearchLinks(HtmlDocument doc)
        {
            var hrefTags = new List<string>();

            var block = doc.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("nav")).ToList();

            if (block.Count == 2)
            {
                var hr = block[1].Descendants("a");
                foreach (HtmlNode link in hr)
                {
                    HtmlAttribute att = link.Attributes["href"];
                    if (att.Value != null && !hrefTags.Contains(att.Value) && att.Value.StartsWith("tracker"))
                        hrefTags.Add(att.Value);
                }
            }
            return hrefTags.Select(link => string.Format("{0}/{1}", HostUrl, link)).ToList();
        }
    }
}
