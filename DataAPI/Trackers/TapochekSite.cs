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
using DataAPI.Database;
using DataAPI.POCO;
using Extensions.Helpers;
using HtmlAgilityPack;
using Interfaces.Enums;
using Interfaces.Models;

namespace DataAPI.Trackers
{
    public class TapochekSite : CommonTracker
    {
        #region Static and Readonly Fields

        private readonly SqLiteDatabase sql;

        #endregion

        #region Fields

        private CredPOCO cred;

        #endregion

        #region Constructors

        public TapochekSite(SqLiteDatabase sql)
        {
            this.sql = sql;
            hostUrl = string.Format("http://{0}", EnumHelper.GetAttributeOfType(SiteType.Tapochek));
            indexUrl = string.Format("{0}/index.php", hostUrl);
            loginUrl = string.Format("{0}/login.php", hostUrl);
            profileUrl = string.Format("{0}/profile.php", hostUrl);
            searchUrl = string.Format("{0}/tracker.php?nm", hostUrl);
            topicUrl = string.Format("{0}/viewtopic.php?t", hostUrl);
            userUrl = string.Format("{0}/tracker.php?rid", hostUrl);
            GetCred();
        }

        #endregion

        #region Static Methods

        private static void FillVideoItemPOCO(VideoItemPOCO item, HtmlNode node, string site)
        {
            IEnumerable<HtmlNode> dl =
                node.Descendants("a").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("small tr-dl"));
            foreach (HtmlNode htmlNode in dl)
            {
                string videoLink = string.Format("{0}{1}", site, htmlNode.Attributes["href"].Value.TrimStart('.'));
                string[] sp = videoLink.Split('=');
                if (sp.Length == 2)
                {
                    item.ID = sp[1];
                }

                // Duration = GetTorrentSize(ScrubHtml(htmlNode.InnerText));
                break;
            }

            IEnumerable<HtmlNode> counts =
                node.Descendants("a").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("genmed"));
            foreach (HtmlNode htmlNode in counts)
            {
                item.Title = HttpUtility.HtmlDecode(htmlNode.InnerText).Trim();
                break;
            }

            IEnumerable<HtmlNode> prov =
                node.Descendants("td")
                    .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("row4 small nowrap"));
            foreach (HtmlNode htmlNode in prov)
            {
                List<HtmlNode> pdate = htmlNode.Descendants("p").ToList();
                if (pdate.Count == 2)
                {
                    item.Timestamp = Convert.ToDateTime(pdate[1].InnerText);
                    break;
                }
            }

            IEnumerable<HtmlNode> seemed =
                node.Descendants("td").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("row4 seedmed"));
            foreach (HtmlNode htmlNode in seemed)
            {
                item.ViewCount = Convert.ToInt32(htmlNode.InnerText);
                break;
            }

            IEnumerable<HtmlNode> med =
                node.Descendants("td").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("row4 small"));
            foreach (HtmlNode htmlNode in med)
            {
                item.Comments = Convert.ToInt32(htmlNode.InnerText);
                break;
            }

            IEnumerable<HtmlNode> user =
                node.Descendants("a").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("med"));
            foreach (HtmlNode htmlNode in user)
            {
                string uid = htmlNode.Attributes["href"].Value;
                string[] sp = uid.Split('=');
                if (sp.Length == 2)
                {
                    item.ParentID = sp[1];
                }

                // VideoOwnerName = htmlNode.InnerText;
                break;
            }

            IEnumerable<HtmlNode> forum =
                node.Descendants("a").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("gen"));
            foreach (HtmlNode htmlNode in forum)
            {
                item.Description = htmlNode.InnerText;
                break;
            }

            // var topic = node.Descendants("a").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("genmed"));
            // foreach (HtmlNode htmlNode in topic)
            // {
            // PlaylistID = string.Format("http://{0}/forum{1}", site, htmlNode.Attributes["href"].Value.TrimStart('.'));
            // break;
            // }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Fill by elements
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task FillChannelNetAsync(IChannel channel)
        {
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
                string link = string.Format("{0}/{1}", hostUrl, img);
                channel.Thumbnail = await SiteHelper.GetStreamFromUrl(link);
            }
        }

        /// <summary>
        ///     Get user releases. 0 - all
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="maxresult"></param>
        /// <returns></returns>
        public async Task<IEnumerable<VideoItemPOCO>> GetChannelItemsAsync(IChannel channel, int maxresult)
        {
            var lst = new List<VideoItemPOCO>();
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
                var item = new VideoItemPOCO();
                FillVideoItemPOCO(item, node, hostUrl);
                if (!string.IsNullOrEmpty(item.ID))
                {
                    lst.Add(item);
                }
            }

            if (maxresult == 0)
            {
                Thread.Sleep(500);

                IEnumerable<string> searchlinks = GetAllSearchLinks(doc);

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
                        var item = new VideoItemPOCO();
                        FillVideoItemPOCO(item, node, hostUrl);
                        if (!string.IsNullOrEmpty(item.ID))
                        {
                            lst.Add(item);
                        }
                    }
                }

                Thread.Sleep(500);
            }

            return lst;
        }

        public Task<ChannelPOCO> GetChannelNetAsync(string channelID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Get user cookie
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task<CookieContainer> GetCookieNetAsync(IChannel channel)
        {
            if (string.IsNullOrEmpty(cred.Login) || string.IsNullOrEmpty(cred.Pass))
            {
                throw new Exception("Please, set login and password");
            }

            var cc = new CookieContainer();
            var req = (HttpWebRequest)WebRequest.Create(loginUrl);
            req.CookieContainer = cc;
            req.Method = WebRequestMethods.Http.Post;
            req.Host = EnumHelper.GetAttributeOfType(channel.Site);
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
            req.Headers.Add("Origin", hostUrl);
            req.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            req.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
            req.Headers.Add("DNT", "1");
            req.Referer = indexUrl;

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

        private IEnumerable<string> GetAllSearchLinks(HtmlDocument doc)
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
            return hrefTags.Select(link => string.Format("{0}/{1}", hostUrl, link)).ToList();
        }

        private async void GetCred()
        {
            cred = await sql.GetCredAsync(SiteType.Tapochek);
        }

        #endregion
    }
}
