using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Interfaces.API;
using Interfaces.POCO;
using Newtonsoft.Json.Linq;
using SitesAPI.POCO;

namespace SitesAPI.Videos
{
    public class YouTubeSite :IYouTubeSite
    {
        private const string Url = "https://www.googleapis.com/youtube/v3/";

        private const string Print = "prettyPrint=false";

        private const string Token = "nextPageToken";

        private const string Privacy = "public";

        private const int ItemsPerPage = 50;

        private const string Key = "AIzaSyDfdgAVDXbepYVGivfbgkknu0kYRbC2XwI";

        private static async Task<string> DownloadStringAsync(Uri uri, int timeOut = 60000)
        {
            string res = null;
            var cancelledOrError = false;

            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.DownloadStringCompleted += (sender, e) =>
                {
                    if (e.Error != null || e.Cancelled)
                    {
                        cancelledOrError = true;
                    }
                    else
                    {
                        res = e.Result;
                    }
                };
                client.DownloadStringAsync(uri);
                var n = DateTime.Now;
                while (res == null && !cancelledOrError && DateTime.Now.Subtract(n).TotalMilliseconds < timeOut)
                {
                    await Task.Delay(100); // wait for respsonse
                }
            }
            if (res == null)
                throw new Exception("Download Error: " + uri.Segments.Last());

            return res;
        }

        public async Task<List<IVideoItemPOCO>> GetChannelItemsAsync(string channelID, int maxResult)
        {
            var res = new List<IVideoItemPOCO>();

            int itemsppage = ItemsPerPage;

            if (maxResult < ItemsPerPage & maxResult != 0)
                itemsppage = maxResult;


            var zap = string.Format("{0}search?&channelId={1}&key={2}&order=date&maxResults={3}&part=snippet&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{4}", Url, channelID,
                Key, itemsppage, Print);

            object pagetoken;

            do
            {
                var str = await DownloadStringAsync(new Uri(zap));

                var jsvideo = await Task.Run(() => JObject.Parse(str));

                pagetoken = jsvideo.SelectToken(Token);

                var sb = new StringBuilder();

                foreach (JToken pair in jsvideo["items"])
                {
                    var tid = pair.SelectToken("id.videoId");
                    if (tid == null)
                        continue;

                    var id = tid.Value<string>();
                    if (res.Select(x => x.ID).Contains(id))
                        continue;

                    var v = new VideoItemPOCO(id);
                    await v.FillFieldsFromGetting(pair);
                    res.Add(v);

                    sb.Append(id).Append(',');

                    if (res.Count == maxResult)
                    {
                        pagetoken = null;
                        break;
                    }
                }

                var ids = sb.ToString().TrimEnd(',');

                zap =
                    string.Format(
                        "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics,status&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount,commentCount),status(privacyStatus))&{3}",
                        Url, ids, Key, Print);

                var det = await DownloadStringAsync(new Uri(zap));

                jsvideo = await Task.Run(() => JObject.Parse(det));

                foreach (JToken pair in jsvideo["items"])
                {
                    var id = pair.SelectToken("id");
                    if (id == null)
                        continue;

                    var item = res.FirstOrDefault(x => x.ID == id.Value<string>()) as VideoItemPOCO;
                    if (item != null)
                    {
                        item.FillFieldsFromDetails(pair);
                    }
                }

                zap = string.Format("{0}search?&channelId={1}&key={2}&order=date&maxResults={3}&pageToken={4}&part=snippet&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{5}",
                    Url, channelID, Key, itemsppage, pagetoken, Print);
            } while (pagetoken != null);

            return res.Where(x => x.Status == Privacy).ToList();
        }

        public async Task<List<IVideoItemPOCO>> GetPopularItemsAsync(string regionID, int maxResult)
        {
            int itemsppage = ItemsPerPage;

            if (maxResult < ItemsPerPage & maxResult != 0)
                itemsppage = maxResult;

            var res = new List<IVideoItemPOCO>();

            var zap =
                string.Format(
                    "{0}videos?chart=mostPopular&regionCode={1}&key={2}&maxResults={3}&part=snippet&safeSearch=none&fields=nextPageToken,items(id,snippet(channelId,title,publishedAt,thumbnails(default(url))))&{4}", Url, regionID,
                    Key, itemsppage, Print);

            object pagetoken;

            do
            {
                var str = await DownloadStringAsync(new Uri(zap));

                var jsvideo = await Task.Run(() => JObject.Parse(str));

                pagetoken = jsvideo.SelectToken(Token);

                var sb = new StringBuilder();

                foreach (JToken pair in jsvideo["items"])
                {
                    var tid = pair.SelectToken("id");
                    if (tid == null)
                        continue;

                    var id = tid.Value<string>();

                    if (res.Select(x => x.ID).Contains(id))
                        continue;

                    var v = new VideoItemPOCO(id);
                    await v.FillFieldsFromGetting(pair);
                    res.Add(v);

                    sb.Append(id).Append(',');

                    if (res.Count == maxResult)
                    {
                        pagetoken = null;
                        break;
                    }
                }

                var ids = sb.ToString().TrimEnd(',');

                var det =
                    string.Format(
                        "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics,status&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount,commentCount),status(privacyStatus))&{3}",
                        Url, ids, Key, Print);

                det = await DownloadStringAsync(new Uri(det));
                jsvideo = await Task.Run(() => JObject.Parse(det));

                foreach (JToken pair in jsvideo["items"])
                {
                    var id = pair.SelectToken("id");
                    if (id == null)
                        continue;

                    var item = res.FirstOrDefault(x => x.ID == id.Value<string>()) as VideoItemPOCO;
                    if (item != null)
                    {
                        item.FillFieldsFromDetails(pair);
                    }
                }

                zap =
                    string.Format(
                        "{0}videos?chart=mostPopular&regionCode={1}&key={2}&maxResults={3}&pageToken={4}&part=snippet&fields=nextPageToken,items(id,snippet(channelId,title,publishedAt,thumbnails(default(url))))&{5}",
                        Url, regionID, Key, itemsppage, pagetoken, Print);
            } while (pagetoken != null);

            return res;
        }

        public async Task<List<IVideoItemPOCO>> SearchItemsAsync(string keyword, int maxResult)
        {
            int itemsppage = ItemsPerPage;

            if (maxResult < ItemsPerPage & maxResult != 0)
                itemsppage = maxResult;

            var res = new List<IVideoItemPOCO>();

            var zap = string.Format("{0}search?&q={1}&key={2}&maxResults={3}&part=snippet&safeSearch=none&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{4}", Url, keyword, Key,
                itemsppage, Print);

            object pagetoken;

            do
            {
                var str = await DownloadStringAsync(new Uri(zap));

                var jsvideo = await Task.Run(() => JObject.Parse(str));

                pagetoken = jsvideo.SelectToken(Token);

                var sb = new StringBuilder();

                foreach (JToken pair in jsvideo["items"])
                {
                    var tid = pair.SelectToken("id.videoId");
                    if (tid == null)
                        continue;

                    var id = tid.Value<string>();

                    if (res.Select(x => x.ID).Contains(id))
                        continue;

                    var v = new VideoItemPOCO(id);
                    await v.FillFieldsFromGetting(pair);
                    res.Add(v);

                    sb.Append(id).Append(',');

                    if (res.Count == maxResult)
                    {
                        pagetoken = null;
                        break;
                    }
                }

                var ids = sb.ToString().TrimEnd(',');

                var det =
                    string.Format(
                        "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount,commentCount))&{3}",
                        Url, ids, Key, Print);

                det = await DownloadStringAsync(new Uri(det));

                jsvideo = await Task.Run(() => JObject.Parse(det));

                foreach (JToken pair in jsvideo["items"])
                {
                    var id = pair.SelectToken("id");
                    if (id == null)
                        continue;

                    var item = res.FirstOrDefault(x => x.ID == id.Value<string>()) as VideoItemPOCO;
                    if (item != null)
                    {
                        item.FillFieldsFromDetails(pair);
                    }
                }

                zap = string.Format(
                    "{0}search?&q={1}&key={2}&maxResults={3}&pageToken={4}&part=snippet&fields=nextPageToken,items(id(videoId),snippet(channelId,title,publishedAt,thumbnails(default(url))))&{5}", Url,
                    keyword, Key, itemsppage, pagetoken, Print);

            } while (pagetoken != null);

            return res;
        }

        public async Task<IVideoItemPOCO> GetVideoItemNetAsync(string videoid)
        {
            var v = new VideoItemPOCO(videoid);

            var zap =
                string.Format(
                    "{0}videos?&id={1}&key={2}&part=snippet,contentDetails,statistics&fields=items(snippet(channelId,title,description,thumbnails(default(url)),publishedAt),contentDetails(duration),statistics(viewCount,commentCount))&{3}",
                    Url, videoid, Key, Print);

            var str = await DownloadStringAsync(new Uri(zap));

            var jsvideo = await Task.Run(() => JObject.Parse(str));

            await v.FillFieldsFromSingleVideo(jsvideo);

            return v;
        }

        public async Task<IChannelPOCO> GetChannelNetAsync(string channelID)
        {
            var zap =
                string.Format(
                    "{0}channels?&id={1}&key={2}&part=snippet&fields=items(snippet(title,description,thumbnails(default(url))))&{3}",
                    Url, channelID, Key, Print);

            var str = await DownloadStringAsync(new Uri(zap));

            var jsvideo = await Task.Run(() => JObject.Parse(str));

            var ch = await ChannelPOCO.CreatePoco(channelID, jsvideo);

            return ch;
        }

        public async Task<List<IPlaylistPOCO>> GetChannelPlaylistNetAsync(string channelID)
        {
            var res = new List<IPlaylistPOCO>();

            object pagetoken;

            var zap =
                string.Format(
                    "{0}playlists?&key={1}&channelId={2}&part=snippet&fields=items(id,snippet(title, description,thumbnails(default(url))))&maxResults={3}&{4}",
                    Url, Key, channelID, ItemsPerPage, Print);

            do
            {
                var str = await DownloadStringAsync(new Uri(zap));

                var jsvideo = await Task.Run(() => JObject.Parse(str));

                pagetoken = jsvideo.SelectToken(Token);

                foreach (JToken pair in jsvideo["items"])
                {
                    var p = new PlaylistPOCO {ChannelID = channelID};

                    await p.FillFieldsFromGetting(pair);

                    if (res.Select(x=>x.ID).Contains(p.ID))
                        continue;

                    res.Add(p);
                }

            } while (pagetoken != null);

            return res;
        }

        public async Task<List<IVideoItemPOCO>> GetPlaylistItemsNetAsync(string plid)
        {
            var res = new List<IVideoItemPOCO>();

            object pagetoken;

            var zap =
                string.Format(
                    "{0}playlistItems?&key={1}&playlistId={2}&part=snippet,status&order=date&fields=nextPageToken,items(snippet(publishedAt,channelId,title,description,thumbnails(default(url)),resourceId(videoId)),status(privacyStatus))&maxResults={3}&{4}",
                    Url, Key, plid, ItemsPerPage, Print);

            do
            {
                var str = await DownloadStringAsync(new Uri(zap));

                var jsvideo = await Task.Run(() => JObject.Parse(str));

                pagetoken = jsvideo.SelectToken(Token);

                var sb = new StringBuilder();

                foreach (JToken pair in jsvideo["items"])
                {
                    var tid = pair.SelectToken("snippet.resourceId.videoId");
                    if (tid == null)
                        continue;

                    var id = tid.Value<string>();
                    if (res.Select(x=>x.ID).Contains(id))
                        continue;

                    var pr = pair.SelectToken("status.privacyStatus");
                    if (pr == null)
                        continue;

                    if (pr.Value<string>() != Privacy)
                        continue;

                    var v = new VideoItemPOCO(id);

                    v.FillFieldsFromPlaylist(pair);
                    res.Add(v);

                    sb.Append(id).Append(',');
                }

                var ids = sb.ToString().TrimEnd(',');

                var det =
                    string.Format(
                        "{0}videos?id={1}&key={2}&part=snippet,contentDetails,statistics&fields=items(id,snippet(description),contentDetails(duration),statistics(viewCount,commentCount))&{3}",
                        Url, ids, Key, Print);

                det = await DownloadStringAsync(new Uri(det));

                jsvideo = await Task.Run(() => JObject.Parse(det));

                foreach (JToken pair in jsvideo["items"])
                {
                    var id = pair.SelectToken("id");
                    if (id == null)
                        continue;

                    var item = res.FirstOrDefault(x => x.ID == id.Value<string>()) as VideoItemPOCO;
                    if (item != null)
                    {
                        item.FillFieldsFromDetails(pair);
                    }
                }

                zap =
                    string.Format(
                        "{0}playlistItems?&key={1}&playlistId={2}&part=snippet,status&pageToken={3}&order=date&fields=nextPageToken,items(snippet(publishedAt,channelId,title,description,thumbnails(default(url)),resourceId(videoId)),status(privacyStatus))&maxResults={4}&{5}",
                        Url, Key, plid, pagetoken, ItemsPerPage, Print);

            } while (pagetoken != null);

            return res;
        }

        public async Task<IPlaylistPOCO> GetPlaylistNetAsync(string id)
        {
            var pl = new PlaylistPOCO
            {
                ID = id,
            };

            var zap =
                string.Format(
                    "{0}playlists?&id={1}&key={2}&part=snippet&fields=items(snippet(title,description,channelId,thumbnails(default(url))))&{3}",
                    Url, id, Key, Print);

            var str = await DownloadStringAsync(new Uri(zap));

            var jsvideo = await Task.Run(() => JObject.Parse(str));

            await pl.FillFieldsFromSingle(jsvideo);

            return pl;
        }

        public async Task<int> GetChannelItemsCountNetAsync(string channelID)
        {
            //var zap =
            //    string.Format(
            //        "{0}channels?id={1}&key={2}&part=statistics&fields=items(statistics(videoCount))&{3}",
            //        Url, channelID, Key, Print);

            var zap =
                string.Format(
                    "{0}search?&channelId={1}&key={2}&maxResults=0&part=snippet&{3}",
                    Url, channelID, Key, Print);

            var str = await DownloadStringAsync(new Uri(zap));

            var jsvideo = await Task.Run(() => JObject.Parse(str));

            //var total = jsvideo.SelectToken("items[0].statistics.videoCount");

            var total = jsvideo.SelectToken("pageInfo.totalResults");

            if (total != null)
            {
                return total.Value<int>();
            }

            throw new Exception(zap);
        }

        public async Task<List<string>> GetChannelItemsIdsListNetAsync(string channelID, int maxResult)
        {
            var res = new List<IVideoItemPOCO>();

            int itemsppage = ItemsPerPage;

            if (maxResult < ItemsPerPage & maxResult != 0)
                itemsppage = maxResult;


            var zap = string.Format("{0}search?&channelId={1}&key={2}&order=date&maxResults={3}&part=snippet&fields=nextPageToken,items(id)&{4}", Url, channelID,
                Key, itemsppage, Print);

            object pagetoken;

            do
            {
                var str = await DownloadStringAsync(new Uri(zap));

                var jsvideo = await Task.Run(() => JObject.Parse(str));

                pagetoken = jsvideo.SelectToken(Token);

                var sb = new StringBuilder();

                foreach (JToken pair in jsvideo["items"])
                {
                    //var tid = pair.SelectToken("id.videoId");
                    //if (tid == null)
                    //    continue;

                    //var id = tid.Value<string>();
                    //if (res.Contains(id))
                    //    continue;

                    //res.Add(id);

                    var tid = pair.SelectToken("id.videoId");
                    if (tid == null)
                        continue;

                    var id = tid.Value<string>();
                    if (res.Select(x => x.ID).Contains(id))
                        continue;

                    var v = new VideoItemPOCO(id);

                    res.Add(v);

                    sb.Append(id).Append(',');

                    if (res.Count == maxResult)
                    {
                        pagetoken = null;
                        break;
                    }
                }

                var ids = sb.ToString().TrimEnd(',');

                zap =
                    string.Format(
                        "{0}videos?id={1}&key={2}&part=status&fields=items(id,status(privacyStatus))&{3}",
                        Url, ids, Key, Print);

                var det = await DownloadStringAsync(new Uri(zap));

                jsvideo = await Task.Run(() => JObject.Parse(det));

                foreach (JToken pair in jsvideo["items"])
                {
                    var id = pair.SelectToken("id");
                    if (id == null)
                        continue;

                    var item = res.FirstOrDefault(x => x.ID == id.Value<string>()) as VideoItemPOCO;
                    if (item != null)
                    {
                        var pr = pair.SelectToken("status.privacyStatus");
                        if (pr == null)
                            continue;

                        item.Status = pr.Value<string>();
                    }
                }

                zap = string.Format("{0}search?&channelId={1}&key={2}&order=date&maxResults={3}&pageToken={4}&part=snippet&fields=nextPageToken,items(id)&{5}",
                    Url, channelID, Key, itemsppage, pagetoken, Print);
            } while (pagetoken != null);

            return res.Where(x => x.Status == Privacy).Select(x => x.ID).ToList();
        }

        public async Task<List<string>> GetPlaylistItemsIdsListNetAsync(string plid)
        {
            var res = new List<string>();

            object pagetoken;

            var zap =
                string.Format(
                    "{0}playlistItems?&key={1}&playlistId={2}&part=snippet,status&order=date&fields=nextPageToken,items(snippet(resourceId(videoId)),status(privacyStatus))&maxResults={3}&{4}",
                    Url, Key, plid, ItemsPerPage, Print);

            do
            {
                var str = await DownloadStringAsync(new Uri(zap));

                var jsvideo = await Task.Run(() => JObject.Parse(str));

                pagetoken = jsvideo.SelectToken(Token);

                foreach (JToken pair in jsvideo["items"])
                {
                    var tid = pair.SelectToken("snippet.resourceId.videoId");
                    if (tid == null)
                        continue;

                    var id = tid.Value<string>();
                    if (res.Contains(id))
                        continue;

                    var pr = pair.SelectToken("status.privacyStatus");
                    if (pr == null)
                        continue;

                    if (pr.Value<string>() == Privacy)
                        res.Add(id);
                }

               zap =
                    string.Format(
                        "{0}playlistItems?&key={1}&playlistId={2}&part=snippet,status&pageToken={3}&order=date&fields=nextPageToken,items(snippet(resourceId(videoId)),status(privacyStatus))&maxResults={4}&{5}",
                        Url, Key, plid, pagetoken, ItemsPerPage, Print);

            } while (pagetoken != null);

            return res;
        }

        public async Task<string> GetChannelIdByUserNameNetAsync(string username)
        {
            var zap =
               string.Format(
                   "{0}channels?&forUsername={1}&key={2}&part=snippet&&fields=items(id)&prettyPrint=false&{3}",
                   Url, username, Key, Print);

            var str = await DownloadStringAsync(new Uri(zap));

            var jsvideo = await Task.Run(() => JObject.Parse(str));

            var id = jsvideo.SelectToken("items[0].id");

            if (id != null)
                return id.Value<string>();

            throw new Exception("Can't get channel ID for username: " + username);
        }
    }
}
