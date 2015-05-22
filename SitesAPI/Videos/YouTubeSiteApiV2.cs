using System;
using System.Collections.Generic;
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
    public class YouTubeSiteApiV2 : IYouTubeSite
    {
        private int _startIndex = 1;

        private const int ItemsPerPage = 25;

        private const string DeprecatedId = "UKY3scPIMd8"; //Youtube API v2 deprecated video

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

        /// <summary>
        /// Получение заданного количество видео с канала, 0 - все записи
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <param name="maxResult">Количество</param>
        /// <returns></returns>
        public async Task<List<IVideoItemPOCO>> GetChannelItemsAsync(string channelID, int maxResult)
        {
            int itemsppage;

            if (maxResult == 0)
            {
                itemsppage = ItemsPerPage;
                maxResult = (await GetChannelItemsCountNetAsync(channelID)) + 1;
            }
            else
                itemsppage = maxResult < ItemsPerPage ? maxResult + 1 : ItemsPerPage;

            var res = new List<IVideoItemPOCO>();
            while (true)
            {
                var zap =
                    string.Format(
                        "https://gdata.youtube.com/feeds/api/users/{0}/uploads?alt=json&start-index={1}&max-results={2}",
                        channelID, _startIndex, itemsppage);

                var str = await DownloadStringAsync(new Uri(zap));

                var jsvideo = await Task.Run(() => JObject.Parse(str));

                foreach (JToken pair in jsvideo["feed"]["entry"])
                {
                    var tid = pair.SelectToken("id.$t");
                    var id = tid != null ? (tid.Value<string>().Split('/').Last() ?? string.Empty) : string.Empty;
                    if (id == string.Empty || id == DeprecatedId)
                        continue;
                    var v = new VideoItemPOCO(id);
                    await v.FillFieldsFromGetting(pair);
                    res.Add(v);
                }

                if (maxResult > res.Count)
                {
                    _startIndex = _startIndex + ItemsPerPage;

                    if (_startIndex == maxResult)
                    {
                        itemsppage = 1;
                        continue;
                    }

                    if (_startIndex < maxResult)
                    {
                        if (maxResult - _startIndex < ItemsPerPage)
                        {
                            itemsppage = maxResult - _startIndex + 1;
                        }

                        continue;
                    }
                }

                _startIndex = 1;
                break;
            }

            return res;
        }

        /// <summary>
        /// Получение списка популярных видео по стране
        /// </summary>
        /// <param name="regionID">Код региона</param>
        /// <param name="maxResult">Желаемое количество записей</param>
        /// <returns></returns>
        public async Task<List<IVideoItemPOCO>> GetPopularItemsAsync(string regionID, int maxResult)
        {
            var itemsppage = maxResult < ItemsPerPage ? maxResult : ItemsPerPage;

            var res = new List<IVideoItemPOCO>();
            while (true)
            {
                var zap =
                    string.Format(
                        "https://gdata.youtube.com/feeds/api/standardfeeds/{0}/most_popular?time=today&v=2&alt=json&start-index={1}&max-results={2}",
                        regionID, _startIndex, itemsppage);

                var str = await DownloadStringAsync(new Uri(zap));

                var jsvideo = await Task.Run(() => JObject.Parse(str));

                foreach (JToken pair in jsvideo["feed"]["entry"])
                {
                    var tid = pair.SelectToken("id.$t");
                    var id = tid != null ? tid.Value<string>().Split(':').Last() ?? string.Empty : string.Empty;
                    if (id == string.Empty || id == DeprecatedId)
                        continue;
                    var v = new VideoItemPOCO(id);
                    await v.FillFieldsFromPopular(pair);
                    res.Add(v);
                }

                if (maxResult > res.Count)
                {
                    _startIndex = _startIndex + ItemsPerPage;

                    if (_startIndex == maxResult)
                    {
                        itemsppage = 1;
                        continue;
                    }

                    if (_startIndex < maxResult)
                    {
                        if (maxResult - _startIndex < ItemsPerPage)
                        {
                            itemsppage = maxResult - _startIndex + 1;
                        }

                        continue;
                    }
                }

                _startIndex = 1;
                break;
            }

            return res;
        }

        /// <summary>
        /// Получение результата запроса
        /// </summary>
        /// <param name="key">Запрос</param>
        /// <param name="maxResult">Желаемое количество записей</param>
        /// <returns></returns>
        public async Task<List<IVideoItemPOCO>> SearchItemsAsync(string key, int maxResult)
        {
            var itemsppage = maxResult < ItemsPerPage ? maxResult : ItemsPerPage;

            var res = new List<IVideoItemPOCO>();
            while (true)
            {
                var zap =
                    string.Format(
                        "https://gdata.youtube.com/feeds/api/videos?q={0}&v=2&alt=json&start-index={1}&max-results={2}",
                        key, _startIndex, itemsppage);

                var str = await DownloadStringAsync(new Uri(zap));

                var jsvideo = await Task.Run(() => JObject.Parse(str));

                foreach (JToken pair in jsvideo["feed"]["entry"])
                {
                    var tid = pair.SelectToken("id.$t");
                    var id = tid != null ? tid.Value<string>().Split(':').Last() ?? string.Empty : string.Empty;
                    if (id == string.Empty || id == DeprecatedId)
                        continue;

                    var v = new VideoItemPOCO(id);
                    await v.FillFieldsFromPopular(pair);
                    res.Add(v);
                }

                if (maxResult > res.Count)
                {
                    _startIndex = _startIndex + ItemsPerPage;

                    if (_startIndex == maxResult)
                    {
                        itemsppage = 1;
                        continue;
                    }

                    if (_startIndex < maxResult)
                    {
                        if (maxResult - _startIndex < ItemsPerPage)
                        {
                            itemsppage = maxResult - _startIndex + 1;
                        }

                        continue;
                    }
                }

                _startIndex = 1;
                break;
            }
            
            return res;
        }

        /// <summary>
        /// Получение видео по ID
        /// </summary>
        /// <param name="videoid">ID видео</param>
        /// <returns></returns>
        public async Task<IVideoItemPOCO> GetVideoItemNetAsync(string videoid)
        {
            var v = new VideoItemPOCO(videoid);

            var zap = string.Format("https://gdata.youtube.com/feeds/api/videos/{0}?v=2&alt=json", videoid);
            
            var str = await DownloadStringAsync(new Uri(zap));

            var jsvideo = await Task.Run(() => JObject.Parse(str));

            await v.FillFieldsFromSingleVideo(jsvideo);

            return v;
        }

        /// <summary>
        /// Получение канала по ID
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        public async Task<IChannelPOCO> GetChannelNetAsync(string channelID)
        {
            var zap = string.Format("https://gdata.youtube.com/feeds/api/users/{0}?v=2&alt=json", channelID);

            var str = await DownloadStringAsync(new Uri(zap));

            var jsvideo = await Task.Run(() => JObject.Parse(str));

            var ch = await ChannelPOCO.CreatePoco(channelID, jsvideo);

            return ch;
        }

        /// <summary>
        /// Получение списка плэйлистов канала
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns>Список плейлистов</returns>
        public async Task<List<IPlaylistPOCO>> GetChannelPlaylistNetAsync(string channelID)
        {
            var res = new List<IPlaylistPOCO>();

            while (true)
            {
                var zap =
                    string.Format(
                        "https://gdata.youtube.com/feeds/api/users/{0}/playlists?v=2&alt=json&start-index={1}&max-results={2}",
                        channelID, _startIndex, ItemsPerPage);
                var str = await DownloadStringAsync(new Uri(zap));

                var jsvideo = await Task.Run(() => JObject.Parse(str));

                int total;
                if (int.TryParse(jsvideo["feed"]["openSearch$totalResults"]["$t"].ToString(), out total))
                {
                    if (total != 0)
                    {
                        foreach (JToken pair in jsvideo["feed"]["entry"])
                        {
                            var p = new PlaylistPOCO();
                            p.FillFieldsFromGetting(pair);
                            res.Add(p);
                        }
                    }

                    if (total > res.Count)
                    {
                        _startIndex = _startIndex + ItemsPerPage;
                        if (_startIndex < total)
                            continue;
                    }

                    _startIndex = 1;
                }
                break;
            }

            return res;
        }

        /// <summary>
        /// Получение списка видео плэйлиста
        /// </summary>
        /// <param name="link">Ссылка</param>
        /// <returns></returns>
        public async Task<List<IVideoItemPOCO>> GetPlaylistItemsNetAsync(string link)
        {
            var res = new List<IVideoItemPOCO>();
            while (true)
            {
                var zap =
                    string.Format(
                        "{0}&start-index={1}&max-results={2}",
                        link, _startIndex, ItemsPerPage);
                var str = await DownloadStringAsync(new Uri(zap));

                var jsvideo = await Task.Run(() => JObject.Parse(str));

                int total;
                if (int.TryParse(jsvideo["feed"]["openSearch$totalResults"]["$t"].ToString(), out total))
                {
                    if (total != 0)
                    {
                        foreach (JToken pair in jsvideo["feed"]["entry"])
                        {
                            var tid = pair.SelectToken("link[0].href");
                            var id = tid != null ? (tid.Value<string>().Split('&').First().Split('=').Last() ?? string.Empty) : string.Empty;
                            if (id == string.Empty || id == DeprecatedId)
                                continue;
                            var v = new VideoItemPOCO(id);
                            v.FillFieldsFromPlaylist(pair);
                            res.Add(v);
                        }
                    }

                    if (total > res.Count)
                    {
                        _startIndex = _startIndex + ItemsPerPage;
                        if (_startIndex < total)
                            continue;
                    }

                    _startIndex = 1;
                }
                break;
            }

            return res;
        }

        /// <summary>
        /// Получение объекта "плэйлист"
        /// </summary>
        /// <param name="id">ID плейлиста</param>
        /// <returns></returns>
        public async Task<IPlaylistPOCO> GetPlaylistNetAsync(string id)
        {
            var pl = new PlaylistPOCO {ID = id};

            var zap = string.Format("https://gdata.youtube.com/feeds/api/playlists/{0}?v=2&alt=json", id);

            var str = await DownloadStringAsync(new Uri(zap));

            var jsvideo = await Task.Run(() => JObject.Parse(str));

            pl.FillFieldsFromSingle(jsvideo);

            return pl;
        }

        /// <summary>
        /// Получение количества видео на канале
        /// </summary>
        /// <param name="channelID">ID канала</param>
        /// <returns></returns>
        public async Task<int> GetChannelItemsCountNetAsync(string channelID)
        {
            var zap =
                    string.Format(
                        "https://gdata.youtube.com/feeds/api/users/{0}/uploads?alt=json&start-index={1}&max-results={2}",
                        channelID, _startIndex, 0);

            var str = await DownloadStringAsync(new Uri(zap));

            var jsvideo = await Task.Run(() => JObject.Parse(str));

            var total = jsvideo.SelectToken("feed.openSearch$totalResults.$t");

            if (total != null)
            {
                return total.Value<int>() - 1;
            }

            throw new Exception(zap);
        }

    }
}
