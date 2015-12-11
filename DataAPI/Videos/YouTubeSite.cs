// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAPI.POCO;
using Extensions;
using Interfaces.Enums;
using Interfaces.Models;
using Interfaces.POCO;
using Newtonsoft.Json.Linq;

namespace DataAPI.Videos
{
    public class YouTubeSite
    {
        #region Constants

        private const int itemsPerPage = 50;
        private const string key = "AIzaSyDfdgAVDXbepYVGivfbgkknu0kYRbC2XwI";
        private const string printType = "prettyPrint=false";
        private const string privacyDef = "other";
        private const string privacyPub = "public";
        private const string privacyUnList = "unlisted";
        private const string token = "nextPageToken";
        private const string url = "https://www.googleapis.com/youtube/v3/";
        private const string youChannel = "channel";
        private const string youUser = "user";

        #endregion

        #region Static and Readonly Fields

        private static string site;

        #endregion

        #region Fields

        private ICred cred;

        #endregion

        #region Properties

        public ICred Cred
        {
            private get
            {
                return cred;
            }
            set
            {
                cred = value;
                if (cred != null)
                {
                    site = cred.SiteAdress;
                }
            }
        }

        #endregion

        #region Static Methods

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
        ///     Get channel by ID
        /// </summary>
        /// <param name="channelID">channel ID</param>
        /// <returns></returns>
        public static async Task<IChannelPOCO> GetChannelNetAsync(string channelID)
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

            ch.Site = site;

            return ch;
        }

        /// <summary>
        ///     Get channel playlists
        /// </summary>
        /// <param name="channelID">channel ID</param>
        /// <returns></returns>
        public static async Task<IEnumerable<IPlaylistPOCO>> GetChannelPlaylistsNetAsync(string channelID)
        {
            var res = new List<IPlaylistPOCO>();

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
        public static async Task<IEnumerable<IPlaylistPOCO>> GetChannelRelatedPlaylistsNetAsync(string channelID)
        {
            var res = new List<IPlaylistPOCO>();
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
                IPlaylistPOCO pl = await GetPlaylistNetAsync(id);
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
        public static async Task<IEnumerable<string>> GetPlaylistItemsIdsListNetAsync(string plid, int maxResult)
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
                    if (prstatus == privacyPub || prstatus == privacyUnList)
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
        ///     Get playlist by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<IPlaylistPOCO> GetPlaylistNetAsync(string id)
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
        ///     Get channel related channels
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<IChannelPOCO>> GetRelatedChannelsByIdAsync(string id)
        {
            var lst = new List<IChannelPOCO>();

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
                    IChannelPOCO ch = await GetChannelNetAsync(jToken.Value<string>());
                    if (!string.IsNullOrEmpty(ch.Title))
                    {
                        lst.Add(ch);
                    }
                }
            }

            return lst;
        }

        /// <summary>
        ///     Get video subtitles
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ISubtitlePOCO>> GetVideoSubtitlesByIdAsync(string id)
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
                select new SubtitlePOCO { Language = lang.Value<string>() }).Cast<ISubtitlePOCO>().ToList();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Get full channel
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        public async Task<IChannelPOCO> GetChannelFullNetAsync(string channelID)
        {
            IChannelPOCO ch = await GetChannelNetAsync(channelID);

            List<IPlaylistPOCO> relatedpls = (await GetChannelRelatedPlaylistsNetAsync(channelID)).ToList();

            IPlaylistPOCO uploads = relatedpls.SingleOrDefault(x => x.SubTitle == "uploads");

            if (uploads == null)
            {
                return ch;
            }

            ch.Items.AddRange(await GetPlaylistItemsNetAsync(uploads.ID));

            // ch.Playlists.AddRange(relatedpls.Where(x => x != uploads)); // Liked, favorites and other
            ch.Playlists.AddRange(await GetChannelPlaylistsNetAsync(channelID));

            foreach (IPlaylistPOCO pl in ch.Playlists)
            {
                IEnumerable<string> plids = await GetPlaylistItemsIdsListNetAsync(pl.ID, 0);
                foreach (string id in plids.Where(id => ch.Items.Select(x => x.ID).Contains(id)))
                {
                    pl.PlaylistItems.Add(id);
                }
            }

            //uploads.PlaylistItems.AddRange(ch.Items.Select(x => x.ID));
            //foreach (IPlaylistPOCO poco in ch.Playlists.Where(poco => poco.SubTitle == "uploads"))
            //{
            //    poco.SubTitle = string.Empty;
            //}
            //ch.Playlists.Add(uploads);

            return ch;
        }

        /// <summary>
        ///     Get channel items, 0 - all items
        /// </summary>
        /// <param name="channelID">channel ID</param>
        /// <param name="maxResult">count</param>
        /// <returns></returns>
        public async Task<IEnumerable<IVideoItemPOCO>> GetChannelItemsAsync(string channelID, int maxResult)
        {
            var res = new List<IVideoItemPOCO>();

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

                    var item = new VideoItemPOCO(id, Cred.Site);
                    await item.FillFieldsFromGetting(pair);
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
                                  "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics,status&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount,commentCount),status(privacyStatus))&{3}", 
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
                        item.Status = privacyDef;
                    }

                    var prstatus = pr.Value<string>();
                    if (prstatus == privacyPub || prstatus == privacyUnList)
                    {
                        item.Status = prstatus;
                    }
                    else
                    {
                        item.Status = privacyDef;
                    }

                    item.FillFieldsFromDetails(pair);
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

            return res.Where(x => x.Status != privacyDef).ToList();
        }

        /// <summary>
        ///     Get channel items count using search (can include hidden items)
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        public async Task<int> GetChannelItemsCountBySearchNetAsync(string channelID)
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
        ///     Get channel items IDs
        /// </summary>
        /// <param name="channelID">channel ID</param>
        /// <param name="maxResult">Count</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetChannelItemsIdsListNetAsync(string channelID, int maxResult)
        {
            var res = new List<IVideoItemPOCO>();

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

                    var item = new VideoItemPOCO(id, Cred.Site);

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
                    if (item != null)
                    {
                        JToken pr = pair.SelectToken("status.privacyStatus");
                        if (pr == null)
                        {
                            continue;
                        }

                        item.Status = pr.Value<string>();
                    }
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

            return res.Where(x => x.Status == privacyPub).Select(x => x.ID).ToList();
        }

        /// <summary>
        ///     Get playlist items
        /// </summary>
        /// <param name="plid"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IVideoItemPOCO>> GetPlaylistItemsNetAsync(string plid)
        {
            var res = new List<IVideoItemPOCO>();

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
                    if (prstatus != privacyPub && prstatus != privacyUnList)
                    {
                        continue;
                    }

                    var item = new VideoItemPOCO(id, Cred.Site);

                    item.FillFieldsFromPlaylist(pair);
                    res.Add(item);
                    sb.Append(id).Append(',');
                }

                string ids = sb.ToString().TrimEnd(',');

                string det =
                    string.Format(
                                  "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount,commentCount))&{3}", 
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

                    var item = res.FirstOrDefault(x => x.ID == id.Value<string>()) as VideoItemPOCO;
                    if (item != null)
                    {
                        item.FillFieldsFromDetails(pair);
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
        ///     Get popular by country
        /// </summary>
        /// <param name="regionID">Country code</param>
        /// <param name="maxResult">count</param>
        /// <returns></returns>
        public async Task<IEnumerable<IVideoItemPOCO>> GetPopularItemsAsync(string regionID, int maxResult)
        {
            int itemsppage = itemsPerPage;

            if (maxResult < itemsPerPage & maxResult != 0)
            {
                itemsppage = maxResult;
            }

            var res = new List<IVideoItemPOCO>();

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

                    var item = new VideoItemPOCO(id, Cred.Site);
                    await item.FillFieldsFromGetting(pair);
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
                                  "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics,status&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount,commentCount),status(privacyStatus))&{3}", 
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

                    var item = res.FirstOrDefault(x => x.ID == id.Value<string>()) as VideoItemPOCO;
                    if (item != null)
                    {
                        item.FillFieldsFromDetails(pair);
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
        ///     Get lite video
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IVideoItemPOCO> GetVideoItemLiteNetAsync(string id)
        {
            var item = new VideoItemPOCO(id, Cred.Site);

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

        /// <summary>
        ///     Get video by id
        /// </summary>
        /// <param name="videoid">video ID</param>
        /// <returns></returns>
        public async Task<IVideoItemPOCO> GetVideoItemNetAsync(string videoid)
        {
            var item = new VideoItemPOCO(videoid, Cred.Site);

            string zap =
                string.Format(
                              "{0}videos?&id={1}&key={2}&part=snippet,contentDetails,statistics&fields=items(snippet(channelId,title,description,thumbnails(default(url)),publishedAt),contentDetails(duration),statistics(viewCount,commentCount))&{3}", 
                    url, 
                    videoid, 
                    key, 
                    printType);

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap));

            JObject jsvideo = JObject.Parse(str);

            await item.FillFieldsFromSingleVideo(jsvideo);

            return item;
        }

        /// <summary>
        ///     Get videos by list id's
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IVideoItemPOCO>> GetVideosListByIdsAsync(List<string> ids)
        {
            var lst = new List<IVideoItemPOCO>();

            var sb = new StringBuilder();

            foreach (string id in ids)
            {
                sb.Append(id).Append(',');
            }

            string res = sb.ToString().TrimEnd(',');

            string zap =
                string.Format(
                              "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics,status&fields=items(id,snippet(description,channelId,title,publishedAt,thumbnails(default(url))),contentDetails(duration),statistics(viewCount,commentCount),status(privacyStatus))&{3}", 
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

                var item = new VideoItemPOCO(id.Value<string>(), Cred.Site);

                await item.FillFieldsFromGetting(pair);

                item.FillFieldsFromDetails(pair);

                JToken pr = pair.SelectToken("status.privacyStatus");
                if (pr == null)
                {
                    item.Status = privacyDef;
                }

                var prstatus = pr.Value<string>();

                if (prstatus == privacyPub || prstatus == privacyUnList)
                {
                    item.Status = prstatus;
                }
                else
                {
                    item.Status = privacyDef;
                }

                if (!lst.Select(x => x.ID).Contains(item.ID))
                {
                    lst.Add(item);
                }
            }

            return lst.Where(x => x.Status != privacyDef).ToList();
        }

        /// <summary>
        ///     Get lite videos by list id's
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IVideoItemPOCO>> GetVideosListByIdsLiteAsync(List<string> ids)
        {
            var lst = new List<IVideoItemPOCO>();

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

                var v = new VideoItemPOCO(id.Value<string>(), Cred.Site);

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
        ///     Get channel id by user input
        /// </summary>
        /// <param name="inputChannelLink"></param>
        /// <returns></returns>
        public async Task<string> ParseChannelLink(string inputChannelLink)
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
                    IVideoItemPOCO vi = await GetVideoItemLiteNetAsync(id);
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
        public async Task<IEnumerable<IVideoItemPOCO>> SearchItemsAsync(string keyword, string region, int maxResult)
        {
            int itemsppage = itemsPerPage;

            if (maxResult < itemsPerPage & maxResult != 0)
            {
                itemsppage = maxResult;
            }

            var res = new List<IVideoItemPOCO>();

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

                    var item = new VideoItemPOCO(id, Cred.Site);
                    await item.FillFieldsFromGetting(pair);
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
                                  "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount,commentCount))&{3}", 
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

                    var item = res.FirstOrDefault(x => x.ID == id.Value<string>()) as VideoItemPOCO;
                    if (item != null)
                    {
                        item.FillFieldsFromDetails(pair);
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

        #endregion
    }
}
