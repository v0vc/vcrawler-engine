using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Interfaces.POCO;
using Newtonsoft.Json.Linq;

namespace SitesAPI.POCO
{
    [DataContract]
    public class VideoItemPOCO : IVideoItemPOCO
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string ParentID { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }
        
        [DataMember]
        public int ViewCount { get; set; }

        [DataMember]
        public int Duration { get; set; }

        [DataMember]
        public int Comments { get; set; }        

        [DataMember]
        public byte[] Thumbnail { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        public async Task FillFieldsFromGetting(JToken record)
        {
            var tid = record.SelectToken("id.$t");
            ID = tid != null ? (tid.Value<string>().Split('/').Last() ?? string.Empty) : string.Empty;
            if (ID == string.Empty)
                return;

            var tpid = record.SelectToken("author[0].uri.$t");
            ParentID = tpid != null ? tpid.Value<string>().Split('/').Last() ?? string.Empty : string.Empty;

            var ttitle = record.SelectToken("title.$t");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var td = record.SelectToken("content.$t");
            Description = td != null ? (td.Value<string>() ?? string.Empty) : string.Empty;

            var stat = record.SelectToken("yt$statistics.viewCount");
            ViewCount = stat != null ? (stat.Value<int?>() ?? 0) : 0;

            var dur = record.SelectToken("media$group.yt$duration.seconds");
            Duration = dur != null ? (dur.Value<int?>() ?? 0) : 0;

            var comm = record.SelectToken("gd$comments.gd$feedLink.countHint");
            Comments = comm != null ? (comm.Value<int?>() ?? 0) : 0;

            var tlink = record.SelectToken("media$group.media$thumbnail[1].url");
            if (tlink != null)
            {
                Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>());
            }

            var tm = record.SelectToken("published.$t");
            Timestamp = tm != null ? (tm.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;
        }

        public async Task FillFieldsFromSingleVideo(JObject record)
        {
            var ttitle = record.SelectToken("entry.title.$t");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var par = record.SelectToken("entry.author[0].uri.$t");
            ParentID = par != null ? (par.Value<string>().Split('/').Last() ?? string.Empty) : string.Empty;

            var desc = record.SelectToken("entry.media$group.media$category[0].$t");
            Description = desc != null ? (desc.Value<string>() ?? string.Empty) : string.Empty;

            var view = record.SelectToken("entry.yt$statistics.viewCount");
            ViewCount = view != null ? (view.Value<int?>() ?? 0) : 0;

            var dur = record.SelectToken("entry.media$group.yt$duration.seconds");
            Duration = dur != null ? (dur.Value<int?>() ?? 0) : 0;

            var comm = record.SelectToken("entry.gd$comments.gd$feedLink.countHint");
            Comments = comm != null ? (comm.Value<int?>() ?? 0) : 0;

            var tlink = record.SelectToken("entry.media$group.media$thumbnail[0].url");
            if (tlink != null)
            {
                Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>());
            }

            var pub = record.SelectToken("entry.published.$t");
            Timestamp = pub != null ? (pub.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;
        }

        public void FillFieldsFromPlaylist(JToken record)
        {
            var tid = record.SelectToken("link[0].href");
            ID = tid != null ? (tid.Value<string>().Split('&').First().Split('=').Last() ?? string.Empty) : string.Empty;
            if (ID == string.Empty)
                return;

            var tpid = record.SelectToken("author[0].uri.$t");
            ParentID = tpid != null ? tpid.Value<string>().Split('/').Last() ?? string.Empty : string.Empty;

            var ttitle = record.SelectToken("title.$t");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var td = record.SelectToken("media$group.media$category[0].$t");
            Description = td != null ? (td.Value<string>() ?? string.Empty) : string.Empty;
        }

        public async Task FillFieldsFromPopular(JToken record)
        {
            var tid = record.SelectToken("id.$t");
            ID = tid != null ? tid.Value<string>().Split(':').Last() ?? string.Empty : string.Empty;
            if (ID == string.Empty) 
                return;

            var tpid = record.SelectToken("author[0].uri.$t");
            ParentID = tpid != null ? tpid.Value<string>().Split('/').Last() ?? string.Empty: string.Empty;

            var ttitle = record.SelectToken("title.$t");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var td = record.SelectToken("media$group.media$category[0].$t");
            Description = td != null ? (td.Value<string>() ?? string.Empty) : string.Empty;

            var view = record.SelectToken("yt$statistics.viewCount");
            ViewCount = view != null ? (view.Value<int?>() ?? 0) : 0;

            var dur = record.SelectToken("media$group.yt$duration.seconds");
            Duration = dur != null ? (dur.Value<int?>() ?? 0) : 0;

            var comm = record.SelectToken("gd$comments.gd$feedLink.countHint");
            Comments = comm != null ? (comm.Value<int?>() ?? 0) : 0;

            var tlink = record.SelectToken("media$group.media$thumbnail[0].url");
            if (tlink != null)
            {
                Thumbnail = await SiteHelper.GetStreamFromUrl(tlink.Value<string>());
            }

            var tm = record.SelectToken("published.$t");
            Timestamp = tm != null ? (tm.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;
        }
    }
}
