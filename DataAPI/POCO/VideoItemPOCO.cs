// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using Interfaces.Enums;

namespace DataAPI.POCO
{
    public class VideoItemPOCO
    {
        #region Constructors

        public VideoItemPOCO(string id, 
            string parentid, 
            string title, 
            int viewcount, 
            int duration, 
            int comments, 
            byte[] thumbnail, 
            DateTime timestamp, 
            byte syncstate)
        {
            ID = id;
            ParentID = parentid;
            Title = title;
            ViewCount = viewcount;
            Duration = duration;
            Comments = comments;
            Thumbnail = thumbnail;
            Timestamp = timestamp;
            SyncState = syncstate;
        }

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

        #region Properties

        public int Comments { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public string ID { get; private set; }
        public string ParentID { get; set; }
        public SiteType Site { get; set; }
        public PrivacyStatus Status { get; set; }
        public byte SyncState { get; private set; }
        public byte[] Thumbnail { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public long ViewCount { get; set; }

        #endregion
    }
}
