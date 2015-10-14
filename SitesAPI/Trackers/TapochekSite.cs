// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Interfaces.API;
using Interfaces.Models;
using Interfaces.POCO;
using SitesAPI.POCO;

namespace SitesAPI.Trackers
{
    public class TapochekSite : ITapochekSite
    {
        #region Static Methods

        private static IEnumerable<string> GetAllSearchLinks(string siteAdress, HtmlDocument doc)
        {
            var hrefTags = new List<string>();

            List<HtmlNode> block =
                doc.DocumentNode.Descendants("div")
                    .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("nav"))
                    .ToList();

            if (block.Count == 2)
            {
                IEnumerable<HtmlNode> hr = block[1].Descendants("a");
                foreach (HtmlNode link in hr)
                {
                    HtmlAttribute att = link.Attributes["href"];
                    if (att.Value != null && !hrefTags.Contains(att.Value) && att.Value.StartsWith("tracker"))
                    {
                        hrefTags.Add(att.Value);
                    }
                }
            }
            return
                hrefTags.Select(link => string.Format("{0}/{1}", string.Format("{0}/index.php", MakeBaseUrl(siteAdress)), link)).ToList();
        }

        private static string MakeBaseUrl(string site)
        {
            return string.Format("http://{0}", site);
        }

        #endregion

        #region ITapochekSite Members

        public async Task FillChannelNetAsync(IChannel channel)
        {
            string profileUrl = string.Format("{0}/profile.php", channel.SiteAdress);
            string zap = string.Format("{0}?mode=viewprofile&u={1}", profileUrl, channel.ID);

            string page = await SiteHelper.DownloadStringWithCookieAsync(new Uri(zap), channel.ChannelCookies);

            var doc = new HtmlDocument();

            doc.LoadHtml(page);

            List<HtmlNode> title =
                doc.DocumentNode.Descendants("p")
                    .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("small mrg_4"))
                    .ToList();

            if (title.Any())
            {
                channel.Title = HttpUtility.HtmlDecode(title[0].InnerText).Trim();
            }

            title =
                doc.DocumentNode.Descendants("p")
                    .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("mrg_4"))
                    .ToList();

            if (title.Any())
            {
                string img = title[0].FirstChild.Attributes["src"].Value;
                string link = string.Format("{0}/{1}", MakeBaseUrl(channel.SiteAdress), img);
                channel.Thumbnail = await SiteHelper.GetStreamFromUrl(link);
            }
        }

        public async Task<IEnumerable<IVideoItemPOCO>> GetChannelItemsAsync(IChannel channel, int maxresult)
        {
            var lst = new List<IVideoItemPOCO>();
            string userUrl = string.Format("{0}/tracker.php?rid", MakeBaseUrl(channel.SiteAdress));
            string zap = string.Format("{0}={1}", userUrl, channel.ID);

            string page = await SiteHelper.DownloadStringWithCookieAsync(new Uri(zap), channel.ChannelCookies);

            var doc = new HtmlDocument();

            doc.LoadHtml(page);

            List<HtmlNode> links =
                doc.DocumentNode.Descendants("tr")
                    .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("tCenter"))
                    .ToList();

            foreach (HtmlNode node in links)
            {
                var vi = new VideoItemPOCO(node, channel.SiteAdress);
                if (!string.IsNullOrEmpty(vi.ID))
                {
                    lst.Add(vi);
                }
            }

            if (maxresult == 0)
            {
                Thread.Sleep(500);

                IEnumerable<string> searchlinks = GetAllSearchLinks(channel.SiteAdress, doc);

                foreach (string link in searchlinks)
                {
                    page = await SiteHelper.DownloadStringWithCookieAsync(new Uri(link), channel.ChannelCookies);

                    doc = new HtmlDocument();

                    doc.LoadHtml(page);

                    links =
                        doc.DocumentNode.Descendants("tr")
                            .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("tCenter"))
                            .ToList();

                    foreach (HtmlNode node in links)
                    {
                        var vi = new VideoItemPOCO(node, channel.SiteAdress);
                        if (!string.IsNullOrEmpty(vi.ID))
                        {
                            lst.Add(vi);
                        }
                    }
                }

                Thread.Sleep(500);
            }

            return lst;
        }

        public async Task<CookieContainer> GetCookieNetAsync(IChannel channel)
        {
            ICred cred = await channel.GetChannelCredentialsAsync();
            if (string.IsNullOrEmpty(cred.Login) || string.IsNullOrEmpty(cred.Pass))
            {
                throw new Exception("Please, set login and password");
            }

            var cc = new CookieContainer();
            string loginUrl = string.Format("{0}/login.php", MakeBaseUrl(channel.SiteAdress));
            var req = (HttpWebRequest)WebRequest.Create(loginUrl);
            req.CookieContainer = cc;
            req.Method = WebRequestMethods.Http.Post;
            req.Host = channel.SiteAdress;
            req.KeepAlive = true;
            string postData = string.Format("login_username={0}&login_password={1}&login=%C2%F5%EE%E4", 
                Uri.EscapeDataString(cred.Login), 
                Uri.EscapeDataString(cred.Pass));
            byte[] data = Encoding.ASCII.GetBytes(postData);
            req.ContentLength = data.Length;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Headers.Add("Cache-Control", "max-age=0");
            req.Headers.Add("Origin", MakeBaseUrl(channel.SiteAdress));
            req.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            req.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
            req.Headers.Add("DNT", "1");
            req.Referer = string.Format("{0}/index.php", MakeBaseUrl(channel.SiteAdress));

            using (Stream stream = await req.GetRequestStreamAsync())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }

            WebResponse resp = await req.GetResponseAsync();

            var res = (HttpWebResponse)resp;
            if (res.StatusCode == HttpStatusCode.OK)
            {
                cc.Add(res.Cookies);
            }
            return cc;
        }

        #endregion
    }
}
