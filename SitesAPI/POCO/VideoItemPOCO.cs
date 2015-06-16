using System;
using System.Threading.Tasks;
using System.Xml;
using Interfaces.POCO;
using Newtonsoft.Json.Linq;

namespace SitesAPI.POCO
{
    public class VideoItemPOCO : IVideoItemPOCO
    {
        public string ID { get; set; }

        public string ParentID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
        
        public int ViewCount { get; set; }

        public int Duration { get; set; }

        public int Comments { get; set; }        

        public byte[] Thumbnail { get; set; }

        public DateTime Timestamp { get; set; }

        public string Status { get; set; }

        public VideoItemPOCO(string id)
        {
            ID = id;
        }

        public void FillFieldsFromDetails(JToken record)
        {
            var desc = record.SelectToken("snippet.description");
            Description = desc != null ? (desc.Value<string>() ?? string.Empty) : string.Empty;

            var stat = record.SelectToken("statistics.viewCount");
            ViewCount = stat != null ? (stat.Value<int?>() ?? 0) : 0;

            var dur = record.SelectToken("contentDetails.duration");
            if (dur != null)
            {
                var ts = XmlConvert.ToTimeSpan(dur.Value<string>());
                Duration = (int)ts.TotalSeconds;
            }
            else
                Duration = 0;

            var comm = record.SelectToken("statistics.commentCount");
            Comments = comm != null ? (comm.Value<int?>() ?? 0) : 0;
        }

        public async Task FillFieldsFromGetting(JToken record)
        {
            var tpid = record.SelectToken("snippet.channelId");
            ParentID = tpid != null ? tpid.Value<string>() ?? string.Empty : string.Empty;

            var ttitle = record.SelectToken("snippet.title");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var tm = record.SelectToken("snippet.publishedAt");
            Timestamp = tm != null ? (tm.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;

            var tlink = record.SelectToken("snippet.thumbnails.default.url");
            if (tlink != null)
            {
                Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>());
            }
        }

        public async Task FillFieldsFromSingleVideo(JObject record)
        {
            var ttitle = record.SelectToken("items[0].snippet.title");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var par = record.SelectToken("items[0].snippet.channelId");
            ParentID = par != null ? (par.Value<string>() ?? string.Empty) : string.Empty;

            var desc = record.SelectToken("items[0].snippet.description");
            Description = desc != null ? (desc.Value<string>() ?? string.Empty) : string.Empty;

            var view = record.SelectToken("items[0].statistics.viewCount");
            ViewCount = view != null ? (view.Value<int?>() ?? 0) : 0;

            var dur = record.SelectToken("items[0].contentDetails.duration");
            if (dur != null)
            {
                var ts = XmlConvert.ToTimeSpan(dur.Value<string>());
                Duration = (int) ts.TotalSeconds;
            }
            else
                Duration = 0;

            var comm = record.SelectToken("items[0].statistics.commentCount");
            Comments = comm != null ? (comm.Value<int?>() ?? 0) : 0;

            var pub = record.SelectToken("items[0].snippet.publishedAt");
            Timestamp = pub != null ? (pub.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;

            var tlink = record.SelectToken("items[0].snippet.thumbnails.default.url");
            if (tlink != null)
            {
                Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>());
            }
        }

        public void FillFieldsFromPlaylist(JToken record)
        {
            var tpid = record.SelectToken("snippet.channelId");
            ParentID = tpid != null ? tpid.Value<string>() ?? string.Empty : string.Empty;

            var ttitle = record.SelectToken("snippet.title");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var tm = record.SelectToken("snippet.publishedAt");
            Timestamp = tm != null ? (tm.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;
        }
    }
}
