// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using HtmlAgilityPack;
using Interfaces.Enums;
using Interfaces.POCO;
using Newtonsoft.Json.Linq;

namespace SitesAPI.POCO
{
    public class VideoItemPOCO : IVideoItemPOCO
    {
        #region Constructors

        public VideoItemPOCO(string id, SiteType site)
        {
            ID = id;
            Site = site;
        }

        public VideoItemPOCO(HtmlNode node, string site)
        {
            IEnumerable<HtmlNode> dl =
                node.Descendants("a").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("small tr-dl"));
            foreach (HtmlNode htmlNode in dl)
            {
                string videoLink = string.Format("{0}{1}", site, htmlNode.Attributes["href"].Value.TrimStart('.'));
                string[] sp = videoLink.Split('=');
                if (sp.Length == 2)
                {
                    ID = sp[1];
                }

                // Duration = GetTorrentSize(ScrubHtml(htmlNode.InnerText));
                break;
            }

            IEnumerable<HtmlNode> counts =
                node.Descendants("a").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("genmed"));
            foreach (HtmlNode htmlNode in counts)
            {
                Title = HttpUtility.HtmlDecode(htmlNode.InnerText).Trim();
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
                    Timestamp = Convert.ToDateTime(pdate[1].InnerText);
                    break;
                }
            }

            IEnumerable<HtmlNode> seemed =
                node.Descendants("td").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("row4 seedmed"));
            foreach (HtmlNode htmlNode in seemed)
            {
                ViewCount = Convert.ToInt32(htmlNode.InnerText);
                break;
            }

            IEnumerable<HtmlNode> med =
                node.Descendants("td").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("row4 small"));
            foreach (HtmlNode htmlNode in med)
            {
                Comments = Convert.ToInt32(htmlNode.InnerText);
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
                    ParentID = sp[1];
                }

                // VideoOwnerName = htmlNode.InnerText;
                break;
            }

            IEnumerable<HtmlNode> forum =
                node.Descendants("a").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("gen"));
            foreach (HtmlNode htmlNode in forum)
            {
                Description = htmlNode.InnerText;
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

        public void FillFieldsFromDetails(JToken record)
        {
            JToken desc = record.SelectToken("snippet.description");
            Description = desc != null ? (desc.Value<string>() ?? string.Empty) : string.Empty;

            JToken stat = record.SelectToken("statistics.viewCount");
            ViewCount = stat != null ? (stat.Value<int?>() ?? 0) : 0;

            JToken dur = record.SelectToken("contentDetails.duration");
            if (dur != null)
            {
                TimeSpan ts = XmlConvert.ToTimeSpan(dur.Value<string>());
                Duration = (int)ts.TotalSeconds;
            }
            else
            {
                Duration = 0;
            }

            JToken comm = record.SelectToken("statistics.commentCount");
            Comments = comm != null ? (comm.Value<int?>() ?? 0) : 0;
        }

        public async Task FillFieldsFromGetting(JToken record)
        {
            JToken tpid = record.SelectToken("snippet.channelId");
            ParentID = tpid != null ? tpid.Value<string>() ?? string.Empty : string.Empty;

            JToken ttitle = record.SelectToken("snippet.title");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            JToken tm = record.SelectToken("snippet.publishedAt");
            Timestamp = tm != null ? (tm.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;

            JToken tlink = record.SelectToken("snippet.thumbnails.default.url");
            if (tlink != null)
            {
                Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>());
            }
        }

        public void FillFieldsFromPlaylist(JToken record)
        {
            JToken tpid = record.SelectToken("snippet.channelId");
            ParentID = tpid != null ? tpid.Value<string>() ?? string.Empty : string.Empty;

            JToken ttitle = record.SelectToken("snippet.title");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            JToken tm = record.SelectToken("snippet.publishedAt");
            Timestamp = tm != null ? (tm.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;
        }

        public async Task FillFieldsFromSingleVideo(JObject record)
        {
            JToken ttitle = record.SelectToken("items[0].snippet.title");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            JToken par = record.SelectToken("items[0].snippet.channelId");
            ParentID = par != null ? (par.Value<string>() ?? string.Empty) : string.Empty;

            JToken desc = record.SelectToken("items[0].snippet.description");
            Description = desc != null ? (desc.Value<string>() ?? string.Empty) : string.Empty;

            JToken view = record.SelectToken("items[0].statistics.viewCount");
            ViewCount = view != null ? (view.Value<int?>() ?? 0) : 0;

            JToken dur = record.SelectToken("items[0].contentDetails.duration");
            if (dur != null)
            {
                TimeSpan ts = XmlConvert.ToTimeSpan(dur.Value<string>());
                Duration = (int)ts.TotalSeconds;
            }
            else
            {
                Duration = 0;
            }

            JToken comm = record.SelectToken("items[0].statistics.commentCount");
            Comments = comm != null ? (comm.Value<int?>() ?? 0) : 0;

            JToken pub = record.SelectToken("items[0].snippet.publishedAt");
            Timestamp = pub != null ? (pub.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;

            JToken tlink = record.SelectToken("items[0].snippet.thumbnails.default.url");
            if (tlink != null)
            {
                Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>());
            }
        }

        #endregion

        #region IVideoItemPOCO Members

        public int Comments { get; private set; }
        public string Description { get; private set; }
        public int Duration { get; private set; }
        public string ID { get; private set; }
        public string ParentID { get; set; }
        public string Status { get; set; }
        public byte[] Thumbnail { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Title { get; private set; }
        public long ViewCount { get; private set; }
        public SiteType Site { get; set; }

        #endregion
    }
}
