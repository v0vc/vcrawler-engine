using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Interfaces.API;
using Interfaces.POCO;
using SitesAPI.POCO;

namespace SitesAPI.Videos
{
    public class YouTubeSiteHtml :IYouTubeSite
    {
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

        public Task<List<IVideoItemPOCO>> GetChannelItemsAsync(string channelID, int maxResult)
        {
            throw new NotImplementedException();
        }

        public Task<List<IVideoItemPOCO>> GetPopularItemsAsync(string regionID, int maxResult)
        {
            throw new NotImplementedException();
        }

        public Task<List<IVideoItemPOCO>> SearchItemsAsync(string key, int maxResult)
        {
            throw new NotImplementedException();
        }

        async Task<IVideoItemPOCO> IYouTubeSite.GetVideoItemNetAsync(string videoid)
        {
            var v = new VideoItemPOCO(videoid);

            var tlink = string.Format("http://img.youtube.com/vi/{0}/1.jpg", videoid);

            v.Thumbnail = await SiteHelper.GetStreamFromUrl(tlink);

            v.Description = "";

            //var zap = string.Format("https://m.youtube.com/watch?={0}", videoid);

            //var client = new HttpClient();

            //var resp = await client.GetAsync(zap);

            //if (resp.IsSuccessStatusCode)
            //{
            //    var str = await resp.Content.ReadAsStringAsync();

            //    var doc = new HtmlDocument();

            //    doc.LoadHtml(str);

            //    var viewcount = doc.DocumentNode.SelectNodes("//*[@class='viewcount']");

            //}


            return v;
        }

        public Task<IChannelPOCO> GetChannelNetAsync(string channelID)
        {
            throw new NotImplementedException();
        }

        public Task<List<IPlaylistPOCO>> GetChannelPlaylistNetAsync(string channelID)
        {
            throw new NotImplementedException();
        }

        public Task<List<IVideoItemPOCO>> GetPlaylistItemsNetAsync(string link)
        {
            throw new NotImplementedException();
        }

        public Task<IPlaylistPOCO> GetPlaylistNetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetChannelItemsCountNetAsync(string channelID)
        {
            throw new NotImplementedException();
        }

    }
}
