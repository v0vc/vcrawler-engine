// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using DataAPI.POCO;
using Extensions;
using Extensions.Helpers;
using Interfaces.Enums;
using Newtonsoft.Json.Linq;

namespace DataAPI.Videos
{
    public class YouTubeSite
    {
        #region Constants

        private const int itemsPerPage = 50;
        private const string key = "AIzaSyDfdgAVDXbepYVGivfbgkknu0kYRbC2XwI";
        private const string printType = "prettyPrint=false";
        private const string token = "nextPageToken";
        private const string url = "https://www.googleapis.com/youtube/v3/";
        private const string youChannel = "channel";
        private const string youUser = "user";

        #endregion

        #region Static Methods

        /// <summary>
        ///     Get full channel
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        public static async Task<ChannelPOCO> GetChannelFullNetAsync(string channelID)
        {
            ChannelPOCO ch = await GetChannelNetAsync(channelID);

            List<PlaylistPOCO> relatedpls = (await GetChannelRelatedPlaylistsNetAsync(channelID)).ToList();

            PlaylistPOCO uploads = relatedpls.SingleOrDefault(x => x.SubTitle == "uploads");

            if (uploads == null)
            {
                return ch;
            }

            ch.Items.AddRange(await GetPlaylistItemsNetAsync(uploads.ID));

            // ch.Playlists.AddRange(relatedpls.Where(x => x != uploads)); // Liked, favorites and other
            ch.Playlists.AddRange(await GetChannelPlaylistsNetAsync(channelID));

            foreach (PlaylistPOCO pl in ch.Playlists)
            {
                IEnumerable<string> plids = await GetPlaylistItemsIdsListNetAsync(pl.ID, 0);
                foreach (string id in plids.Where(id => ch.Items.Select(x => x.ID).Contains(id)))
                {
                    pl.PlaylistItems.Add(id);
                }
            }

            // uploads.PlaylistItems.AddRange(ch.Items.Select(x => x.ID));
            // foreach (IPlaylistPOCO poco in ch.Playlists.Where(poco => poco.SubTitle == "uploads"))
            // {
            // poco.SubTitle = string.Empty;
            // }
            // ch.Playlists.Add(uploads);
            return ch;
        }

        /// <summary>
        ///     Get channel ID by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static async Task<string> GetChannelIdByUserNameNetAsync(string username)
        {
            string zap = string.Format("{0}channels?&forUsername={1}&key={2}&part=snippet&&fields=items(id)&prettyPrint=false&{3}", 
                url, 
                username, 
                key, 
                printType);

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(str);

            JToken id = jsvideo.SelectToken("items[0].id");

            if (id != null)
            {
                return id.Value<string>();
            }

            throw new Exception("Can't get channel ID for username: " + username);
        }

        /// <summary>
        ///     Get channel items, 0 - all items
        /// </summary>
        /// <param name="channelID">channel ID</param>
        /// <param name="maxResult">count</param>
        /// <returns></returns>
        public static async Task<IEnumerable<VideoItemPOCO>> GetChannelItemsAsync(string channelID, int maxResult)
        {
            var res = new List<VideoItemPOCO>();

            int itemsppage = itemsPerPage;

            if (maxResult < itemsPerPage & maxResult != 0)
            {
                itemsppage = maxResult;
            }

            string zap =
                string.Format(
                              "{0}search?&channelId={1}&key={2}&order=date&maxResults={3}&part=snippet&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{4}", 
                    url, 
                    channelID, 
                    key, 
                    itemsppage, 
                    printType);

            object pagetoken;

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

                JObject jsvideo = JObject.Parse(str);

                pagetoken = jsvideo.SelectToken(token);

                var sb = new StringBuilder();

                foreach (JToken pair in jsvideo["items"])
                {
                    JToken tid = pair.SelectToken("id.videoId");
                    if (tid == null)
                    {
                        continue;
                    }

                    var id = tid.Value<string>();
                    if (res.Select(x => x.ID).Contains(id))
                    {
                        continue;
                    }

                    var item = new VideoItemPOCO(id, SiteType.YouTube);
                    await FillFieldsFromGetting(item, pair);
                    res.Add(item);

                    sb.Append(id).Append(',');

                    if (res.Count == maxResult)
                    {
                        pagetoken = null;
                        break;
                    }
                }

                string ids = sb.ToString().TrimEnd(',');

                zap =
                    string.Format(
                                  "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics,status&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount),status(privacyStatus))&{3}", 
                        url, 
                        ids, 
                        key, 
                        printType);

                string det = await SiteHelper.DownloadStringAsync(new Uri(zap));

                jsvideo = JObject.Parse(det);

                foreach (JToken pair in jsvideo["items"])
                {
                    JToken id = pair.SelectToken("id");
                    if (id == null)
                    {
                        continue;
                    }

                    VideoItemPOCO item = res.FirstOrDefault(x => x.ID == id.Value<string>());

                    if (item == null)
                    {
                        continue;
                    }

                    JToken pr = pair.SelectToken("status.privacyStatus");
                    var prstatus = pr.Value<string>();
                    item.Status = EnumHelper.GetValueFromDescription<PrivacyStatus>(prstatus);
                    FillFieldsFromDetails(item, pair);
                }

                zap =
                    string.Format(
                                  "{0}search?&channelId={1}&key={2}&order=date&maxResults={3}&pageToken={4}&part=snippet&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{5}", 
                        url, 
                        channelID, 
                        key, 
                        itemsppage, 
                        pagetoken, 
                        printType);
            }
            while (pagetoken != null);

            return res.Where(x => x.Status != PrivacyStatus.Private).ToList();
        }

        /// <summary>
        ///     Get channel items count using search (can include hidden items)
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        public static async Task<int> GetChannelItemsCountBySearchNetAsync(string channelID)
        {
            string zap = string.Format("{0}search?&channelId={1}&key={2}&maxResults=0&part=snippet&{3}", url, channelID, key, printType);

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(str);

            JToken total = jsvideo.SelectToken("pageInfo.totalResults");

            if (total == null)
            {
                throw new Exception(zap);
            }
            return total.Value<int>();
        }

        /// <summary>
        ///     Get channel items count
        /// </summary>
        /// <param name="channelID">channel ID</param>
        /// <returns></returns>
        public static async Task<int> GetChannelItemsCountNetAsync(string channelID)
        {
            string zap = string.Format("{0}channels?id={1}&key={2}&part=statistics&fields=items(statistics(videoCount))&{3}", 
                url, 
                channelID, 
                key, 
                printType);

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(str);

            JToken total = jsvideo.SelectToken("items[0].statistics.videoCount");

            if (total == null)
            {
                throw new Exception(zap);
            }
            return total.Value<int>();
        }

        /// <summary>
        ///     Get channel items IDs
        /// </summary>
        /// <param name="channelID">channel ID</param>
        /// <param name="maxResult">Count</param>
        /// <returns></returns>
        public static async Task<IEnumerable<string>> GetChannelItemsIdsListNetAsync(string channelID, int maxResult)
        {
            var res = new List<VideoItemPOCO>();

            int itemsppage = itemsPerPage;

            if (maxResult < itemsPerPage & maxResult != 0)
            {
                itemsppage = maxResult;
            }

            string zap =
                string.Format(
                              "{0}search?&channelId={1}&key={2}&order=date&maxResults={3}&part=snippet&fields=nextPageToken,items(id)&{4}", 
                    url, 
                    channelID, 
                    key, 
                    itemsppage, 
                    printType);

            object pagetoken;

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

                JObject jsvideo = JObject.Parse(str);

                pagetoken = jsvideo.SelectToken(token);

                var sb = new StringBuilder();

                foreach (JToken pair in jsvideo["items"])
                {
                    JToken tid = pair.SelectToken("id.videoId");
                    if (tid == null)
                    {
                        continue;
                    }

                    var id = tid.Value<string>();
                    if (res.Select(x => x.ID).Contains(id))
                    {
                        continue;
                    }

                    var item = new VideoItemPOCO(id, SiteType.YouTube);

                    res.Add(item);

                    sb.Append(id).Append(',');

                    if (res.Count == maxResult)
                    {
                        pagetoken = null;
                        break;
                    }
                }

                string ids = sb.ToString().TrimEnd(',');

                zap = string.Format("{0}videos?id={1}&key={2}&part=status&fields=items(id,status(privacyStatus))&{3}", 
                    url, 
                    ids, 
                    key, 
                    printType);

                string det = await SiteHelper.DownloadStringAsync(new Uri(zap));

                jsvideo = JObject.Parse(det);

                foreach (JToken pair in jsvideo["items"])
                {
                    JToken id = pair.SelectToken("id");
                    if (id == null)
                    {
                        continue;
                    }

                    var item = res.FirstOrDefault(x => x.ID == id.Value<string>()) as VideoItemPOCO;
                    if (item == null)
                    {
                        continue;
                    }
                    JToken pr = pair.SelectToken("status.privacyStatus");
                    if (pr == null)
                    {
                        continue;
                    }
                    var status = pr.Value<string>();
                    item.Status = EnumHelper.GetValueFromDescription<PrivacyStatus>(status);
                }

                zap =
                    string.Format(
                                  "{0}search?&channelId={1}&key={2}&order=date&maxResults={3}&pageToken={4}&part=snippet&fields=nextPageToken,items(id)&{5}", 
                        url, 
                        channelID, 
                        key, 
                        itemsppage, 
                        pagetoken, 
                        printType);
            }
            while (pagetoken != null);

            return res.Where(x => x.Status == PrivacyStatus.Public).Select(x => x.ID).ToList();
        }

        /// <summary>
        ///     Get channel by ID
        /// </summary>
        /// <param name="channelID">channel ID</param>
        /// <returns></returns>
        public static async Task<ChannelPOCO> GetChannelNetAsync(string channelID)
        {
            string zap =
                string.Format(
                              "{0}channels?&id={1}&key={2}&part=snippet&fields=items(snippet(title,description,thumbnails(default(url))))&{3}", 
                    url, 
                    channelID, 
                    key, 
                    printType);

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(str);

            ChannelPOCO ch = await ChannelPOCO.CreatePoco(channelID, jsvideo);

            ch.Site = SiteType.YouTube;

            return ch;
        }

        /// <summary>
        ///     Get channel playlists ids
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        public static async Task<List<string>> GetChannelPlaylistsIdsNetAsync(string channelID)
        {
            var res = new List<string>();

            object pagetoken;

            string zap = string.Format("{0}playlists?&key={1}&channelId={2}&part=snippet&fields=items(id)&maxResults={3}&{4}",
                url,
                key,
                channelID,
                itemsPerPage,
                printType);

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

                JObject jsvideo = JObject.Parse(str);

                pagetoken = jsvideo.SelectToken(token);

                res.AddRange(from pair in jsvideo["items"]
                    select pair.SelectToken("id")
                    into tid where tid != null select tid.Value<string>());
            }
            while (pagetoken != null);

            return res;
        }

        /// <summary>
        ///     Get channel playlists
        /// </summary>
        /// <param name="channelID">channel ID</param>
        /// <returns></returns>
        public static async Task<IEnumerable<PlaylistPOCO>> GetChannelPlaylistsNetAsync(string channelID)
        {
            var res = new List<PlaylistPOCO>();

            object pagetoken;

            string zap =
                string.Format(
                              "{0}playlists?&key={1}&channelId={2}&part=snippet&fields=items(id,snippet(title, description,thumbnails(default(url))))&maxResults={3}&{4}", 
                    url, 
                    key, 
                    channelID, 
                    itemsPerPage, 
                    printType);

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

                JObject jsvideo = JObject.Parse(str);

                pagetoken = jsvideo.SelectToken(token);

                foreach (JToken pair in jsvideo["items"])
                {
                    var p = new PlaylistPOCO(channelID, SiteType.YouTube);

                    await p.FillFieldsFromGetting(pair);

                    if (res.Select(x => x.ID).Contains(p.ID))
                    {
                        continue;
                    }
                    p.ChannelID = channelID;
                    res.Add(p);
                }
            }
            while (pagetoken != null);

            return res;
        }

        /// <summary>
        ///     Get channel related playlists
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<PlaylistPOCO>> GetChannelRelatedPlaylistsNetAsync(string channelID)
        {
            var res = new List<PlaylistPOCO>();
            string zap =
                string.Format("{0}channels?&key={1}&id={2}&part=contentDetails&fields=items(contentDetails(relatedPlaylists))&{3}", 
                    url, 
                    key, 
                    channelID, 
                    printType);

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap));
            JObject jsvideo = JObject.Parse(str);
            JToken par = jsvideo.SelectToken("items[0].contentDetails.relatedPlaylists");
            if (par == null)
            {
                return res;
            }
            foreach (JToken jToken in par)
            {
                if (!jToken.HasValues)
                {
                    continue;
                }
                string[] values = jToken.ToString().Split(':');
                if (values.Length != 2)
                {
                    continue;
                }
                string name = values[0].Trim(' ', '\"');
                string id = values[1].Trim(' ', '\"');
                PlaylistPOCO pl = await GetPlaylistNetAsync(id);
                if (string.IsNullOrEmpty(pl.Title))
                {
                    continue;
                }
                pl.SubTitle = name;
                res.Add(pl);
            }
            return res;
        }

        /// <summary>
        ///     Get playlist items count
        /// </summary>
        /// <param name="plId"></param>
        /// <returns></returns>
        public static async Task<int> GetPlaylistItemsCountNetAsync(string plId)
        {
            string zap = string.Format("{0}playlists?id={1}&key={2}&part=contentDetails&fields=items(contentDetails(itemCount))&{3}", 
                url, 
                plId, 
                key, 
                printType);

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(str);

            JToken total = jsvideo.SelectToken("items[0].contentDetails.itemCount");

            if (total == null)
            {
                throw new Exception(zap);
            }
            return total.Value<int>();
        }

        /// <summary>
        ///     Get playlist items id's
        /// </summary>
        /// <param name="plid">playlist ID</param>
        /// <param name="maxResult"></param>
        /// <returns></returns>
        public static async Task<List<string>> GetPlaylistItemsIdsListNetAsync(string plid, int maxResult)
        {
            var res = new List<string>();

            int itemsppage = itemsPerPage;

            if (maxResult < itemsPerPage & maxResult != 0)
            {
                itemsppage = maxResult;
            }

            object pagetoken;

            string zap =
                string.Format(
                              "{0}playlistItems?&key={1}&playlistId={2}&part=snippet,status&order=date&fields=nextPageToken,items(snippet(resourceId(videoId)),status(privacyStatus))&maxResults={3}&{4}", 
                    url, 
                    key, 
                    plid, 
                    itemsppage, 
                    printType);

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

                JObject jsvideo = JObject.Parse(str);

                pagetoken = jsvideo.SelectToken(token);

                foreach (JToken pair in jsvideo["items"])
                {
                    JToken tid = pair.SelectToken("snippet.resourceId.videoId");
                    if (tid == null)
                    {
                        continue;
                    }

                    var id = tid.Value<string>();
                    if (res.Contains(id))
                    {
                        continue;
                    }

                    JToken pr = pair.SelectToken("status.privacyStatus");
                    if (pr == null)
                    {
                        continue;
                    }
                    var prstatus = pr.Value<string>();
                    if (prstatus == EnumHelper.GetAttributeOfType(PrivacyStatus.Public)
                        || prstatus == EnumHelper.GetAttributeOfType(PrivacyStatus.Unlisted))
                    {
                        res.Add(id);
                    }

                    if (res.Count == maxResult)
                    {
                        pagetoken = null;
                        break;
                    }
                }

                zap =
                    string.Format(
                                  "{0}playlistItems?&key={1}&playlistId={2}&part=snippet,status&pageToken={3}&order=date&fields=nextPageToken,items(snippet(resourceId(videoId)),status(privacyStatus))&maxResults={4}&{5}", 
                        url, 
                        key, 
                        plid, 
                        pagetoken, 
                        itemsPerPage, 
                        printType);
            }
            while (pagetoken != null);

            return res;
        }

        /// <summary>
        ///     Get playlist items
        /// </summary>
        /// <param name="plid"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<VideoItemPOCO>> GetPlaylistItemsNetAsync(string plid)
        {
            var res = new List<VideoItemPOCO>();

            object pagetoken;

            string zap =
                string.Format(
                              "{0}playlistItems?&key={1}&playlistId={2}&part=snippet,status&order=date&fields=nextPageToken,items(snippet(publishedAt,channelId,title,description,thumbnails(default(url)),resourceId(videoId)),status(privacyStatus))&maxResults={3}&{4}", 
                    url, 
                    key, 
                    plid, 
                    itemsPerPage, 
                    printType);

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

                JObject jsvideo = JObject.Parse(str);

                pagetoken = jsvideo.SelectToken(token);

                var sb = new StringBuilder();

                foreach (JToken pair in jsvideo["items"])
                {
                    JToken tid = pair.SelectToken("snippet.resourceId.videoId");
                    if (tid == null)
                    {
                        continue;
                    }

                    var id = tid.Value<string>();
                    if (res.Select(x => x.ID).Contains(id))
                    {
                        continue;
                    }

                    JToken pr = pair.SelectToken("status.privacyStatus");
                    if (pr == null)
                    {
                        continue;
                    }

                    var prstatus = pr.Value<string>();
                    if (prstatus != EnumHelper.GetAttributeOfType(PrivacyStatus.Public)
                        && prstatus != EnumHelper.GetAttributeOfType(PrivacyStatus.Unlisted))
                    {
                        continue;
                    }

                    var item = new VideoItemPOCO(id, SiteType.YouTube);
                    await FillFieldsFromPlaylist(item, pair);
                    res.Add(item);
                    sb.Append(id).Append(',');
                }

                string ids = sb.ToString().TrimEnd(',');

                string det =
                    string.Format(
                                  "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount))&{3}", 
                        url, 
                        ids, 
                        key, 
                        printType);

                det = await SiteHelper.DownloadStringAsync(new Uri(det));

                jsvideo = JObject.Parse(det);

                foreach (JToken pair in jsvideo["items"])
                {
                    JToken id = pair.SelectToken("id");
                    if (id == null)
                    {
                        continue;
                    }

                    VideoItemPOCO item = res.FirstOrDefault(x => x.ID == id.Value<string>());
                    if (item != null)
                    {
                        FillFieldsFromDetails(item, pair);
                    }
                }

                zap =
                    string.Format(
                                  "{0}playlistItems?&key={1}&playlistId={2}&part=snippet,status&pageToken={3}&order=date&fields=nextPageToken,items(snippet(publishedAt,channelId,title,description,thumbnails(default(url)),resourceId(videoId)),status(privacyStatus))&maxResults={4}&{5}", 
                        url, 
                        key, 
                        plid, 
                        pagetoken, 
                        itemsPerPage, 
                        printType);
            }
            while (pagetoken != null);

            return res;
        }

        /// <summary>
        ///     Get playlist by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<PlaylistPOCO> GetPlaylistNetAsync(string id)
        {
            var pl = new PlaylistPOCO(id, SiteType.YouTube);

            string zap =
                string.Format(
                              "{0}playlists?&id={1}&key={2}&part=snippet&fields=items(snippet(title,description,channelId,thumbnails(default(url))))&{3}", 
                    url, 
                    id, 
                    key, 
                    printType);

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(str);

            await pl.FillFieldsFromSingle(jsvideo);

            return pl;
        }

        /// <summary>
        ///     Get popular by country
        /// </summary>
        /// <param name="regionID">Country code</param>
        /// <param name="maxResult">count</param>
        /// <returns></returns>
        public static async Task<IEnumerable<VideoItemPOCO>> GetPopularItemsAsync(string regionID, int maxResult)
        {
            int itemsppage = itemsPerPage;

            if (maxResult < itemsPerPage & maxResult != 0)
            {
                itemsppage = maxResult;
            }

            var res = new List<VideoItemPOCO>();

            string zap =
                string.Format(
                              "{0}videos?chart=mostPopular&regionCode={1}&key={2}&maxResults={3}&part=snippet&safeSearch=none&fields=nextPageToken,items(id,snippet(channelId,title,publishedAt,thumbnails(default(url))))&{4}", 
                    url, 
                    regionID, 
                    key, 
                    itemsppage, 
                    printType);

            object pagetoken;

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

                JObject jsvideo = JObject.Parse(str);

                pagetoken = jsvideo.SelectToken(token);

                var sb = new StringBuilder();

                foreach (JToken pair in jsvideo["items"])
                {
                    JToken tid = pair.SelectToken("id");
                    if (tid == null)
                    {
                        continue;
                    }

                    var id = tid.Value<string>();

                    if (res.Select(x => x.ID).Contains(id))
                    {
                        continue;
                    }

                    var item = new VideoItemPOCO(id, SiteType.YouTube);
                    await FillFieldsFromGetting(item, pair);
                    res.Add(item);

                    sb.Append(id).Append(',');

                    if (res.Count == maxResult)
                    {
                        pagetoken = null;
                        break;
                    }
                }

                string ids = sb.ToString().TrimEnd(',');

                string det =
                    string.Format(
                                  "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics,status&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount),status(privacyStatus))&{3}", 
                        url, 
                        ids, 
                        key, 
                        printType);

                det = await SiteHelper.DownloadStringAsync(new Uri(det));
                jsvideo = JObject.Parse(det);

                foreach (JToken pair in jsvideo["items"])
                {
                    JToken id = pair.SelectToken("id");
                    if (id == null)
                    {
                        continue;
                    }

                    VideoItemPOCO item = res.FirstOrDefault(x => x.ID == id.Value<string>());
                    if (item != null)
                    {
                        FillFieldsFromDetails(item, pair);
                    }
                }

                zap =
                    string.Format(
                                  "{0}videos?chart=mostPopular&regionCode={1}&key={2}&maxResults={3}&pageToken={4}&part=snippet&fields=nextPageToken,items(id,snippet(channelId,title,publishedAt,thumbnails(default(url))))&{5}", 
                        url, 
                        regionID, 
                        key, 
                        itemsppage, 
                        pagetoken, 
                        printType);
            }
            while (pagetoken != null);

            return res;
        }

        /// <summary>
        ///     Get channel related channels
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ChannelPOCO>> GetRelatedChannelsByIdAsync(string id)
        {
            var lst = new List<ChannelPOCO>();

            string zap =
                string.Format(
                              "{0}channels?id={1}&key={2}&part=brandingSettings&fields=items(brandingSettings(channel(featuredChannelsUrls)))&{3}", 
                    url, 
                    id, 
                    key, 
                    printType);

            string det = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(det);

            JToken par = jsvideo.SelectToken("items[0].brandingSettings.channel.featuredChannelsUrls");

            if (par != null)
            {
                foreach (JToken jToken in par)
                {
                    ChannelPOCO ch = await GetChannelNetAsync(jToken.Value<string>());
                    if (!string.IsNullOrEmpty(ch.Title))
                    {
                        lst.Add(ch);
                    }
                }
            }

            return lst;
        }

        /// <summary>
        ///     Get video by id
        /// </summary>
        /// <param name="videoid">video ID</param>
        /// <returns></returns>
        public static async Task<VideoItemPOCO> GetVideoItemNetAsync(string videoid)
        {
            var item = new VideoItemPOCO(videoid, SiteType.YouTube);

            string zap =
                string.Format(
                              "{0}videos?&id={1}&key={2}&part=snippet,contentDetails,statistics&fields=items(snippet(channelId,title,description,thumbnails(default(url)),publishedAt),contentDetails(duration),statistics(viewCount))&{3}", 
                    url, 
                    videoid, 
                    key, 
                    printType);

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(str);

            await FillFieldsFromSingleVideo(item, jsvideo);

            return item;
        }

        /// <summary>
        ///     Get videos by list id's
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<VideoItemPOCO>> GetVideosListByIdsAsync(IEnumerable<string> ids)
        {
            var lst = new List<VideoItemPOCO>();

            var sb = new StringBuilder();

            foreach (string id in ids)
            {
                sb.Append(id).Append(',');
            }

            string res = sb.ToString().TrimEnd(',');

            string zap =
                string.Format(
                              "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics,status&fields=items(id,snippet(description,channelId,title,publishedAt,thumbnails(default(url))),contentDetails(duration),statistics(viewCount),status(privacyStatus))&{3}", 
                    url, 
                    res, 
                    key, 
                    printType);

            string det = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(det);

            foreach (JToken pair in jsvideo["items"])
            {
                JToken id = pair.SelectToken("id");
                if (id == null)
                {
                    continue;
                }

                var item = new VideoItemPOCO(id.Value<string>(), SiteType.YouTube);

                await FillFieldsFromGetting(item, pair);

                FillFieldsFromDetails(item, pair);

                JToken pr = pair.SelectToken("status.privacyStatus");
                var prstatus = pr.Value<string>();
                item.Status = EnumHelper.GetValueFromDescription<PrivacyStatus>(prstatus);
                if (!lst.Select(x => x.ID).Contains(item.ID))
                {
                    lst.Add(item);
                }
            }

            return lst.Where(x => x.Status != PrivacyStatus.Private).ToList();
        }

        /// <summary>
        ///     Get lite videos by list id's
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static async Task<List<VideoItemPOCO>> GetVideosListByIdsLiteAsync(IEnumerable<string> ids)
        {
            var lst = new List<VideoItemPOCO>();

            var sb = new StringBuilder();

            foreach (string id in ids)
            {
                sb.Append(id).Append(',');
            }

            string res = sb.ToString().TrimEnd(',');

            string zap = string.Format("{0}videos?&id={1}&key={2}&part=snippet&fields=items(id,snippet(channelId))&{3}", 
                url, 
                res, 
                key, 
                printType);

            string det = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(det);

            foreach (JToken pair in jsvideo["items"])
            {
                JToken id = pair.SelectToken("id");
                if (id == null)
                {
                    continue;
                }

                var v = new VideoItemPOCO(id.Value<string>(), SiteType.YouTube);

                JToken pid = pair.SelectToken("snippet.channelId");

                if (pid != null)
                {
                    v.ParentID = pid.Value<string>();
                }

                if (!string.IsNullOrEmpty(v.ID) & !string.IsNullOrEmpty(v.ParentID))
                {
                    lst.Add(v);
                }
            }

            return lst;
        }

        /// <summary>
        ///     Get video subtitles
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<SubtitlePOCO>> GetVideoSubtitlesByIdAsync(string id)
        {
            string zap = string.Format("{0}captions?&videoId={1}&key={2}&part=snippet&fields=items(snippet(language))&{3}", 
                url, 
                id, 
                key, 
                printType);

            string det = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(det);

            return (from pair in jsvideo["items"]
                select pair.SelectToken("snippet.language")
                into lang
                where lang != null
                select new SubtitlePOCO { Language = lang.Value<string>() }).Cast<SubtitlePOCO>().ToList();
        }

        /// <summary>
        ///     Get channel id by user input
        /// </summary>
        /// <param name="inputChannelLink"></param>
        /// <returns></returns>
        public static async Task<string> ParseChannelLink(string inputChannelLink)
        {
            string parsedChannelId = string.Empty;
            string[] sp = inputChannelLink.Split('/');
            if (sp.Length > 1)
            {
                if (sp.Contains(youUser))
                {
                    int indexuser = Array.IndexOf(sp, youUser);
                    if (indexuser < 0)
                    {
                        return string.Empty;
                    }

                    string user = sp[indexuser + 1];
                    parsedChannelId = await GetChannelIdByUserNameNetAsync(user);
                }
                else if (sp.Contains(youChannel))
                {
                    int indexchannel = Array.IndexOf(sp, youChannel);
                    if (indexchannel < 0)
                    {
                        return string.Empty;
                    }

                    parsedChannelId = sp[indexchannel + 1];
                }
                else
                {
                    var regex = new Regex(CommonExtensions.YouRegex);
                    Match match = regex.Match(inputChannelLink);
                    if (!match.Success)
                    {
                        return parsedChannelId;
                    }
                    string id = match.Groups[1].Value;
                    VideoItemPOCO vi = await GetVideoItemLiteNetAsync(id);
                    parsedChannelId = vi.ParentID;
                }
            }
            else
            {
                try
                {
                    parsedChannelId = await GetChannelIdByUserNameNetAsync(inputChannelLink);
                }
                catch
                {
                    parsedChannelId = inputChannelLink;
                }
            }
            return parsedChannelId;
        }

        /// <summary>
        ///     Search
        /// </summary>
        /// <param name="keyword">key</param>
        /// <param name="region">region</param>
        /// <param name="maxResult">count</param>
        /// <returns></returns>
        public static async Task<IEnumerable<VideoItemPOCO>> SearchItemsAsync(string keyword, string region, int maxResult)
        {
            int itemsppage = itemsPerPage;

            if (maxResult < itemsPerPage & maxResult != 0)
            {
                itemsppage = maxResult;
            }

            var res = new List<VideoItemPOCO>();

            string zap =
                string.Format(
                              "{0}search?&q={1}&key={2}&maxResults={3}&regionCode={4}&part=snippet&safeSearch=none&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{5}", 
                    url, 
                    keyword, 
                    key, 
                    itemsppage, 
                    region, 
                    printType);

            object pagetoken;

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

                JObject jsvideo = JObject.Parse(str);

                pagetoken = jsvideo.SelectToken(token);

                var sb = new StringBuilder();

                foreach (JToken pair in jsvideo["items"])
                {
                    JToken tid = pair.SelectToken("id.videoId");
                    if (tid == null)
                    {
                        continue;
                    }

                    var id = tid.Value<string>();

                    if (res.Select(x => x.ID).Contains(id))
                    {
                        continue;
                    }

                    var item = new VideoItemPOCO(id, SiteType.YouTube);
                    await FillFieldsFromGetting(item, pair);
                    res.Add(item);

                    sb.Append(id).Append(',');

                    if (res.Count == maxResult)
                    {
                        pagetoken = null;
                        break;
                    }
                }

                string ids = sb.ToString().TrimEnd(',');

                string det =
                    string.Format(
                                  "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount))&{3}", 
                        url, 
                        ids, 
                        key, 
                        printType);

                det = await SiteHelper.DownloadStringAsync(new Uri(det));

                jsvideo = JObject.Parse(det);

                foreach (JToken pair in jsvideo["items"])
                {
                    JToken id = pair.SelectToken("id");
                    if (id == null)
                    {
                        continue;
                    }

                    VideoItemPOCO item = res.FirstOrDefault(x => x.ID == id.Value<string>());
                    if (item != null)
                    {
                        FillFieldsFromDetails(item, pair);
                    }
                }

                zap =
                    string.Format(
                                  "{0}search?&q={1}&key={2}&maxResults={3}&regionCode={4}&pageToken={5}&part=snippet&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{6}", 
                        url, 
                        keyword, 
                        key, 
                        itemsppage, 
                        region, 
                        pagetoken, 
                        printType);
            }
            while (pagetoken != null);

            return res;
        }

        private static void FillFieldsFromDetails(VideoItemPOCO item, JToken record)
        {
            JToken desc = record.SelectToken("snippet.description");
            item.Description = desc != null ? (desc.Value<string>() ?? string.Empty) : string.Empty;

            JToken stat = record.SelectToken("statistics.viewCount");
            item.ViewCount = stat != null ? (stat.Value<long?>() ?? 0) : 0;

            JToken dur = record.SelectToken("contentDetails.duration");
            if (dur != null)
            {
                TimeSpan ts = XmlConvert.ToTimeSpan(dur.Value<string>());
                item.Duration = (int)ts.TotalSeconds;
            }
            else
            {
                item.Duration = 0;
            }

            item.Comments = 0;
        }

        private static async Task FillFieldsFromGetting(VideoItemPOCO item, JToken record)
        {
            JToken tpid = record.SelectToken("snippet.channelId");
            item.ParentID = tpid != null ? tpid.Value<string>() ?? string.Empty : string.Empty;

            JToken ttitle = record.SelectToken("snippet.title");
            item.Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            JToken tm = record.SelectToken("snippet.publishedAt");
            item.Timestamp = tm != null ? (tm.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;

            JToken tlink = record.SelectToken("snippet.thumbnails.default.url");
            if (tlink != null)
            {
                item.Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>());
            }
        }

        private static async Task FillFieldsFromPlaylist(VideoItemPOCO item, JToken record)
        {
            JToken tpid = record.SelectToken("snippet.channelId");
            item.ParentID = tpid != null ? tpid.Value<string>() ?? string.Empty : string.Empty;

            JToken ttitle = record.SelectToken("snippet.title");
            item.Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            JToken tm = record.SelectToken("snippet.publishedAt");
            item.Timestamp = tm != null ? (tm.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;

            JToken tlink = record.SelectToken("snippet.thumbnails.default.url");
            if (tlink != null)
            {
                item.Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>());
            }
        }

        private static async Task FillFieldsFromSingleVideo(VideoItemPOCO item, JToken record)
        {
            JToken ttitle = record.SelectToken("items[0].snippet.title");
            item.Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            JToken par = record.SelectToken("items[0].snippet.channelId");
            item.ParentID = par != null ? (par.Value<string>() ?? string.Empty) : string.Empty;

            JToken desc = record.SelectToken("items[0].snippet.description");
            item.Description = desc != null ? (desc.Value<string>() ?? string.Empty) : string.Empty;

            JToken view = record.SelectToken("items[0].statistics.viewCount");
            item.ViewCount = view != null ? (view.Value<long?>() ?? 0) : 0;

            JToken dur = record.SelectToken("items[0].contentDetails.duration");
            if (dur != null)
            {
                TimeSpan ts = XmlConvert.ToTimeSpan(dur.Value<string>());
                item.Duration = (int)ts.TotalSeconds;
            }
            else
            {
                item.Duration = 0;
            }

            // JToken comm = record.SelectToken("items[0].statistics.commentCount");
            // Comments = comm != null ? (comm.Value<int?>() ?? 0) : 0;
            item.Comments = 0;

            JToken pub = record.SelectToken("items[0].snippet.publishedAt");
            item.Timestamp = pub != null ? (pub.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;

            JToken tlink = record.SelectToken("items[0].snippet.thumbnails.default.url");
            if (tlink != null)
            {
                item.Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>());
            }
        }

        /// <summary>
        ///     Get lite video
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static async Task<VideoItemPOCO> GetVideoItemLiteNetAsync(string id)
        {
            var item = new VideoItemPOCO(id, SiteType.YouTube);

            string zap = string.Format("{0}videos?&id={1}&key={2}&part=snippet&fields=items(snippet(channelId))&{3}", 
                url, 
                id, 
                key, 
                printType);

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(str);

            JToken par = jsvideo.SelectToken("items[0].snippet.channelId");

            item.ParentID = par != null ? (par.Value<string>() ?? string.Empty) : string.Empty;

            return item;
        }

        #endregion
    }
}
