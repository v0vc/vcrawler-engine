// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
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
    public static class YouTubeSite
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
            ChannelPOCO ch = await GetChannelNetAsync(channelID).ConfigureAwait(false);

            List<PlaylistPOCO> relatedpls = await GetChannelRelatedPlaylistsNetAsync(channelID).ConfigureAwait(false);

            PlaylistPOCO uploads = relatedpls.SingleOrDefault(x => x.SubTitle == "uploads");

            if (uploads == null)
            {
                return ch;
            }

            List<VideoItemPOCO> items = await GetPlaylistItemsNetAsync(uploads.ID).ConfigureAwait(false);

            ch.Items.AddRange(items);

            // ch.Playlists.AddRange(relatedpls.Where(x => x != uploads)); // Liked, favorites and other
            ch.Playlists.AddRange(await GetChannelPlaylistsNetAsync(channelID).ConfigureAwait(false));

            foreach (PlaylistPOCO pl in ch.Playlists)
            {
                IEnumerable<string> plids = await GetPlaylistItemsIdsListNetAsync(pl.ID, 0).ConfigureAwait(false);
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
            string zap = $"{url}channels?&forUsername={username}&key={key}&part=snippet&&fields=items(id)&prettyPrint=false&{printType}";

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

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
        /// <param name="skipfirst"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<VideoItemPOCO>> GetChannelItemsAsync(string channelID, int maxResult, bool skipfirst = false)
        {
            var res = new List<VideoItemPOCO>();

            int itemsppage = itemsPerPage;

            if (maxResult < itemsPerPage & maxResult != 0)
            {
                itemsppage = maxResult;
            }

            string zap =
                $"{url}search?&channelId={channelID}&key={key}&order=date&maxResults={itemsppage}&part=snippet&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{printType}";

            object pagetoken;

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

                JObject jsvideo = JObject.Parse(str);

                pagetoken = jsvideo.SelectToken(token);

                if (skipfirst)
                {
                    skipfirst = false;
                    zap =
                        $"{url}search?&channelId={channelID}&key={key}&order=date&maxResults={itemsppage}&pageToken={pagetoken}&part=snippet&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{printType}";
                    continue;
                }

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

                    var item = new VideoItemPOCO(id);
                    await FillFieldsFromGetting(item, pair).ConfigureAwait(false);
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
                    $"{url}videos?id={ids}&key={key}&part=snippet,contentDetails,statistics,status&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount),status(privacyStatus))&{printType}";

                string det = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

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
                    $"{url}search?&channelId={channelID}&key={key}&order=date&maxResults={itemsppage}&pageToken={pagetoken}&part=snippet&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{printType}";
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
            string zap = $"{url}search?&channelId={channelID}&key={key}&maxResults=0&part=snippet&{printType}";

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

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
            string zap = $"{url}channels?id={channelID}&key={key}&part=statistics&fields=items(statistics(videoCount))&{printType}";

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

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
        public static async Task<List<string>> GetChannelItemsIdsListNetAsync(string channelID, int maxResult)
        {
            var res = new List<VideoItemPOCO>();

            int itemsppage = itemsPerPage;

            if (maxResult < itemsPerPage & maxResult != 0)
            {
                itemsppage = maxResult;
            }

            string zap =
                $"{url}search?&channelId={channelID}&key={key}&order=date&maxResults={itemsppage}&part=snippet&fields=nextPageToken,items(id)&{printType}";

            object pagetoken;

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

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

                    var item = new VideoItemPOCO(id);

                    res.Add(item);

                    sb.Append(id).Append(',');

                    if (res.Count == maxResult)
                    {
                        pagetoken = null;
                        break;
                    }
                }

                string ids = sb.ToString().TrimEnd(',');

                zap = $"{url}videos?id={ids}&key={key}&part=status&fields=items(id,status(privacyStatus))&{printType}";

                string det = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

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
                    if (pr == null)
                    {
                        continue;
                    }
                    var status = pr.Value<string>();
                    item.Status = EnumHelper.GetValueFromDescription<PrivacyStatus>(status);
                }

                zap =
                    $"{url}search?&channelId={channelID}&key={key}&order=date&maxResults={itemsppage}&pageToken={pagetoken}&part=snippet&fields=nextPageToken,items(id)&{printType}";
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
                $"{url}channels?&id={channelID}&key={key}&part=snippet&fields=items(snippet(title,description,thumbnails(default(url))))&{printType}";

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

            JObject jsvideo = JObject.Parse(str);

            ChannelPOCO ch = await ChannelPOCO.CreatePoco(channelID, jsvideo).ConfigureAwait(false);

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

            string zap =
                $"{url}playlists?&key={key}&channelId={channelID}&part=snippet&fields=items(id)&maxResults={itemsPerPage}&{printType}";

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

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
        public static async Task<List<PlaylistPOCO>> GetChannelPlaylistsNetAsync(string channelID)
        {
            var res = new List<PlaylistPOCO>();

            object pagetoken;

            string zap =
                $"{url}playlists?&key={key}&channelId={channelID}&part=snippet&fields=items(id,snippet(title, description,thumbnails(default(url))))&maxResults={itemsPerPage}&{printType}";

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

                JObject jsvideo = JObject.Parse(str);

                pagetoken = jsvideo.SelectToken(token);

                foreach (JToken pair in jsvideo["items"])
                {
                    var p = new PlaylistPOCO(channelID, SiteType.YouTube);

                    await p.FillFieldsFromGetting(pair).ConfigureAwait(false);

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
        public static async Task<List<PlaylistPOCO>> GetChannelRelatedPlaylistsNetAsync(string channelID)
        {
            var res = new List<PlaylistPOCO>();
            string zap =
                $"{url}channels?&key={key}&id={channelID}&part=contentDetails&fields=items(contentDetails(relatedPlaylists))&{printType}";

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);
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
                PlaylistPOCO pl = await GetPlaylistNetAsync(id).ConfigureAwait(false);
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
            string zap = $"{url}playlists?id={plId}&key={key}&part=contentDetails&fields=items(contentDetails(itemCount))&{printType}";

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

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
                $"{url}playlistItems?&key={key}&playlistId={plid}&part=snippet,status&order=date&fields=nextPageToken,items(snippet(resourceId(videoId)),status(privacyStatus))&maxResults={itemsppage}&{printType}";

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

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
                    $"{url}playlistItems?&key={key}&playlistId={plid}&part=snippet,status&pageToken={pagetoken}&order=date&fields=nextPageToken,items(snippet(resourceId(videoId)),status(privacyStatus))&maxResults={itemsPerPage}&{printType}";
            }
            while (pagetoken != null);

            return res;
        }

        /// <summary>
        ///     Get playlist items
        /// </summary>
        /// <param name="plid"></param>
        /// <returns></returns>
        public static async Task<List<VideoItemPOCO>> GetPlaylistItemsNetAsync(string plid)
        {
            var res = new List<VideoItemPOCO>();

            object pagetoken;

            string zap =
                $"{url}playlistItems?&key={key}&playlistId={plid}&part=snippet,status&order=date&fields=nextPageToken,items(snippet(publishedAt,channelId,title,description,thumbnails(default(url)),resourceId(videoId)),status(privacyStatus))&maxResults={itemsPerPage}&{printType}";

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

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

                    var item = new VideoItemPOCO(id);
                    await FillFieldsFromPlaylist(item, pair).ConfigureAwait(false);
                    res.Add(item);
                    sb.Append(id).Append(',');
                }

                string ids = sb.ToString().TrimEnd(',');

                string det =
                    $"{url}videos?id={ids}&key={key}&part=snippet,contentDetails,statistics&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount))&{printType}";

                det = await SiteHelper.DownloadStringAsync(new Uri(det)).ConfigureAwait(false);

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
                    $"{url}playlistItems?&key={key}&playlistId={plid}&part=snippet,status&pageToken={pagetoken}&order=date&fields=nextPageToken,items(snippet(publishedAt,channelId,title,description,thumbnails(default(url)),resourceId(videoId)),status(privacyStatus))&maxResults={itemsPerPage}&{printType}";
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
                $"{url}playlists?&id={id}&key={key}&part=snippet&fields=items(snippet(title,description,channelId,thumbnails(default(url))))&{printType}";

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

            JObject jsvideo = JObject.Parse(str);

            await pl.FillFieldsFromSingle(jsvideo).ConfigureAwait(false);

            return pl;
        }

        /// <summary>
        ///     Get popular by country
        /// </summary>
        /// <param name="regionID">Country code</param>
        /// <param name="maxResult">count</param>
        /// <returns></returns>
        public static async Task<List<VideoItemPOCO>> GetPopularItemsAsync(string regionID, int maxResult)
        {
            int itemsppage = itemsPerPage;

            if (maxResult < itemsPerPage & maxResult != 0)
            {
                itemsppage = maxResult;
            }

            var res = new List<VideoItemPOCO>();

            string zap =
                $"{url}videos?chart=mostPopular&regionCode={regionID}&key={key}&maxResults={itemsppage}&part=snippet&safeSearch=none&fields=nextPageToken,items(id,snippet(channelId,title,publishedAt,thumbnails(default(url))))&{printType}";

            object pagetoken;

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

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

                    var item = new VideoItemPOCO(id);
                    await FillFieldsFromGetting(item, pair).ConfigureAwait(false);
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
                    $"{url}videos?id={ids}&key={key}&part=snippet,contentDetails,statistics,status&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount),status(privacyStatus))&{printType}";

                det = await SiteHelper.DownloadStringAsync(new Uri(det)).ConfigureAwait(false);
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
                    $"{url}videos?chart=mostPopular&regionCode={regionID}&key={key}&maxResults={itemsppage}&pageToken={pagetoken}&part=snippet&fields=nextPageToken,items(id,snippet(channelId,title,publishedAt,thumbnails(default(url))))&{printType}";
            }
            while (pagetoken != null);

            return res;
        }

        /// <summary>
        ///     Get channel related channels
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<List<ChannelPOCO>> GetRelatedChannelsByIdAsync(string id)
        {
            var lst = new List<ChannelPOCO>();

            string zap =
                $"{url}channels?id={id}&key={key}&part=brandingSettings&fields=items(brandingSettings(channel(featuredChannelsUrls)))&{printType}";

            string det = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

            JObject jsvideo = JObject.Parse(det);

            JToken par = jsvideo.SelectToken("items[0].brandingSettings.channel.featuredChannelsUrls");

            if (par == null)
            {
                return lst;
            }
            foreach (JToken jToken in par)
            {
                ChannelPOCO ch = await GetChannelNetAsync(jToken.Value<string>()).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(ch.Title))
                {
                    lst.Add(ch);
                }
            }

            return lst;
        }

        /// <summary>
        ///     Get video comments
        /// </summary>
        /// <param name="videoID"></param>
        /// <param name="maxResult"></param>
        /// <returns></returns>
        public static async Task<List<string>> GetVideoCommentsNetAsync(string videoID, int maxResult)
        {
            var res = new List<string>();

            int itemsppage = itemsPerPage;

            if (maxResult < itemsPerPage & maxResult != 0)
            {
                itemsppage = maxResult;
            }

            string zap =
                $"{url}commentThreads?videoId={videoID}&key={key}&maxResults={itemsppage}&part=snippet&fields=nextPageToken,items(snippet(topLevelComment(snippet(authorDisplayName,textDisplay,authorChannelUrl,authorProfileImageUrl,publishedAt))))&{printType}";

            object pagetoken;
            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

                JObject jsvideo = JObject.Parse(str);

                pagetoken = jsvideo.SelectToken(token);
            }
            while (pagetoken != null);

            return res;
        }

        /// <summary>
        ///     Get video by id
        /// </summary>
        /// <param name="videoid">video ID</param>
        /// <returns></returns>
        public static async Task<VideoItemPOCO> GetVideoItemNetAsync(string videoid)
        {
            var item = new VideoItemPOCO(videoid);

            string zap =
                $"{url}videos?&id={videoid}&key={key}&part=snippet,contentDetails,statistics&fields=items(snippet(channelId,title,description,thumbnails(default(url)),publishedAt),contentDetails(duration),statistics(viewCount))&{printType}";

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

            JObject jsvideo = JObject.Parse(str);

            await FillFieldsFromSingleVideo(item, jsvideo).ConfigureAwait(false);

            return item;
        }

        /// <summary>
        ///     Get videos by list id's
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static async Task<List<VideoItemPOCO>> GetVideosListByIdsAsync(IEnumerable<string> ids)
        {
            var lst = new List<VideoItemPOCO>();

            var sb = new StringBuilder();

            foreach (string id in ids)
            {
                sb.Append(id).Append(',');
            }

            string res = sb.ToString().TrimEnd(',');

            string zap =
                $"{url}videos?id={res}&key={key}&part=snippet,contentDetails,statistics,status&fields=items(id,snippet(description,channelId,title,publishedAt,thumbnails(default(url))),contentDetails(duration),statistics(viewCount),status(privacyStatus))&{printType}";

            string det = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

            JObject jsvideo = JObject.Parse(det);

            foreach (JToken pair in jsvideo["items"])
            {
                JToken id = pair.SelectToken("id");
                if (id == null)
                {
                    continue;
                }

                var item = new VideoItemPOCO(id.Value<string>());

                await FillFieldsFromGetting(item, pair).ConfigureAwait(false);

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

            string zap = $"{url}videos?&id={res}&key={key}&part=snippet&fields=items(id,snippet(channelId))&{printType}";

            string det = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

            JObject jsvideo = JObject.Parse(det);

            foreach (JToken pair in jsvideo["items"])
            {
                JToken id = pair.SelectToken("id");
                if (id == null)
                {
                    continue;
                }

                var v = new VideoItemPOCO(id.Value<string>());

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
        public static async Task<List<SubtitlePOCO>> GetVideoSubtitlesByIdAsync(string id)
        {
            string zap = $"{url}captions?&videoId={id}&key={key}&part=snippet&fields=items(snippet(language))&{printType}";

            string det = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

            JObject jsvideo = JObject.Parse(det);

            return (from pair in jsvideo["items"]
                select pair.SelectToken("snippet.language")
                into lang
                where lang != null
                select new SubtitlePOCO { Language = lang.Value<string>() }).ToList();
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
                    parsedChannelId = await GetChannelIdByUserNameNetAsync(user).ConfigureAwait(false);
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
                    VideoItemPOCO vi = await GetVideoItemLiteNetAsync(id).ConfigureAwait(false);
                    parsedChannelId = vi.ParentID;
                }
            }
            else
            {
                try
                {
                    parsedChannelId = await GetChannelIdByUserNameNetAsync(inputChannelLink).ConfigureAwait(false);
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
        public static async Task<List<VideoItemPOCO>> SearchItemsAsync(string keyword, string region, int maxResult)
        {
            int itemsppage = itemsPerPage;

            if (maxResult < itemsPerPage & maxResult != 0)
            {
                itemsppage = maxResult;
            }

            var res = new List<VideoItemPOCO>();

            string zap =
                $"{url}search?&q={keyword}&key={key}&maxResults={itemsppage}&regionCode={region}&part=snippet&safeSearch=none&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{printType}";

            object pagetoken;

            do
            {
                string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

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

                    var item = new VideoItemPOCO(id);
                    await FillFieldsFromGetting(item, pair).ConfigureAwait(false);
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
                    $"{url}videos?id={ids}&key={key}&part=snippet,contentDetails,statistics&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount))&{printType}";

                det = await SiteHelper.DownloadStringAsync(new Uri(det)).ConfigureAwait(false);

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
                    $"{url}search?&q={keyword}&key={key}&maxResults={itemsppage}&regionCode={region}&pageToken={pagetoken}&part=snippet&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{printType}";
            }
            while (pagetoken != null);

            return res;
        }

        /// <summary>
        ///     Set video rating
        /// </summary>
        /// <param name="videoID"></param>
        /// <param name="rateType"></param>
        /// <returns></returns>
        public static async Task<string> SetVideoRatingNetAsync(string videoID, string rateType)
        {
            string zap = $"{url}videos/rate?id={videoID}&rating={rateType}&key={key}";
            return await SiteHelper.PostMethod(zap);
        }

        /// <summary>
        ///     Get video view count
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        public static async Task<long> GetVideoViewCountNetAsync(string videoId)
        {
            string zap =
                $"{url}videos?&id={videoId}&key={key}&part=statistics&fields=items(statistics(viewCount))&{printType}";

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

            JObject jsvideo = JObject.Parse(str);

            JToken view = jsvideo.SelectToken("items[0].statistics.viewCount");
            return view != null ? (view.Value<long?>() ?? 0) : 0;
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
                item.Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>()).ConfigureAwait(false);
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
                item.Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>()).ConfigureAwait(false);
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
                item.Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>()).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Get lite video
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static async Task<VideoItemPOCO> GetVideoItemLiteNetAsync(string id)
        {
            var item = new VideoItemPOCO(id);

            string zap = $"{url}videos?&id={id}&key={key}&part=snippet&fields=items(snippet(channelId))&{printType}";

            string str = await SiteHelper.DownloadStringAsync(new Uri(zap)).ConfigureAwait(false);

            JObject jsvideo = JObject.Parse(str);

            JToken par = jsvideo.SelectToken("items[0].snippet.channelId");

            item.ParentID = par != null ? (par.Value<string>() ?? string.Empty) : string.Empty;

            return item;
        }

        #endregion
    }
}
