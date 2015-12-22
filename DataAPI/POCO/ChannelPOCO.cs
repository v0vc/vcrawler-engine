// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Threading.Tasks;
using Extensions.Helpers;
using Newtonsoft.Json.Linq;

namespace DataAPI.POCO
{
    public class ChannelPOCO
    {
        #region Constructors

        public ChannelPOCO(string id, string title, byte[] thumbnail, string site, int countnew)
        {
            ID = id;
            Title = title;
            Thumbnail = thumbnail;
            Site = site;
            Countnew = countnew;
        }

        private ChannelPOCO()
        {
        }

        #endregion

        #region Static Methods

        public static async Task<ChannelPOCO> CreatePoco(string id, JObject record)
        {
            var ch = new ChannelPOCO { ID = id, Items = new List<VideoItemPOCO>(), Playlists = new List<PlaylistPOCO>() };

            JToken ttitle = record.SelectToken("items[0].snippet.title");
            ch.Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            JToken sub = record.SelectToken("items[0].snippet.description");
            ch.SubTitle = sub != null ? (sub.Value<string>() ?? string.Empty) : string.Empty;

            JToken link = record.SelectToken("items[0].snippet.thumbnails.default.url");
            if (link != null)
            {
                ch.Thumbnail = await SiteHelper.GetStreamFromUrl(link.Value<string>());
            }

            return ch;
        }

        #endregion

        #region IChannelPOCO Members

        public string ID { get; private set; }
        public string Site { get; set; }
        public string SubTitle { get; private set; }
        public byte[] Thumbnail { get; private set; }
        public string Title { get; private set; }
        public int Countnew { get; private set; }
        public List<PlaylistPOCO> Playlists { get; private set; }
        public List<VideoItemPOCO> Items { get; private set; }

        #endregion
    }
}
