// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Threading.Tasks;
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.POCO;
using Newtonsoft.Json.Linq;

namespace DataAPI.POCO
{
    public class PlaylistPOCO : IPlaylistPOCO
    {
        #region Constructors

        public PlaylistPOCO(string id, string title, string subtitle, byte[] thumbnail, string channelid)
        {
            ID = id;
            Title = title;
            SubTitle = subtitle;
            Thumbnail = thumbnail;
            ChannelID = channelid;
            PlaylistItems = new List<string>();
        }

        public PlaylistPOCO(string id, SiteType site)
        {
            ID = id;
            Site = site;
            PlaylistItems = new List<string>();
        }

        #endregion

        #region Methods

        public async Task FillFieldsFromGetting(JToken record)
        {
            JToken tid = record.SelectToken("id");
            ID = tid != null ? (tid.Value<string>() ?? string.Empty) : string.Empty;

            JToken ttitle = record.SelectToken("snippet.title");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            JToken desc = record.SelectToken("snippet.description");
            SubTitle = desc != null ? (desc.Value<string>() ?? string.Empty) : string.Empty;

            JToken link = record.SelectToken("snippet.thumbnails.default.url");
            if (link != null)
            {
                Thumbnail = await SiteHelper.GetStreamFromUrl(link.Value<string>());
            }
        }

        public async Task FillFieldsFromSingle(JObject record)
        {
            JToken ttitle = record.SelectToken("items[0].snippet.title");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            JToken sum = record.SelectToken("items[0].snippet.description");
            SubTitle = sum != null ? (sum.Value<string>() ?? string.Empty) : string.Empty;

            JToken tpid = record.SelectToken("items[0].snippet.channelId");
            ChannelID = tpid != null ? tpid.Value<string>() ?? string.Empty : string.Empty;

            JToken link = record.SelectToken("items[0].snippet.thumbnails.default.url");
            if (link != null)
            {
                Thumbnail = await SiteHelper.GetStreamFromUrl(link.Value<string>());
            }
        }

        #endregion

        #region IPlaylistPOCO Members

        public string ChannelID { get; set; }
        public string ID { get; set; }
        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; private set; }
        public string Title { get; private set; }
        public List<string> PlaylistItems { get; set; }
        public SiteType Site { get; set; }

        #endregion
    }
}
