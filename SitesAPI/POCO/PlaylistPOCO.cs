using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Interfaces.POCO;
using Newtonsoft.Json.Linq;

namespace SitesAPI.POCO
{
    [DataContract]
    public class PlaylistPOCO : IPlaylistPOCO
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string SubTitle { get; set; }

        [DataMember]
        public string Link { get; set; }

        [DataMember]
        public string ChannelID { get; set; }

        public void FillFieldsFromGetting(JToken pair)
        {
            var tid = pair.SelectToken("yt$playlistId.$t");
            ID = tid != null ? (tid.Value<string>() ?? string.Empty) : string.Empty;
            if (ID == string.Empty)
                return;

            var ttitle = pair.SelectToken("title.$t");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var sum = pair.SelectToken("summary.$t");
            SubTitle = sum != null ? (sum.Value<string>() ?? string.Empty) : string.Empty;

            var link = pair.SelectToken("content.src");
            Link = link != null ? string.Format("{0}&alt=json", link.Value<string>()) : string.Empty;

            var tpid = pair.SelectToken("author[0].uri.$t");
            ChannelID = tpid != null ? tpid.Value<string>().Split('/').Last() ??string.Empty : string.Empty;
        }

        public void FillFieldsFromSingle(JObject jsvideo)
        {
            var ttitle = jsvideo.SelectToken("feed.title.$t");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var sum = jsvideo.SelectToken("feed.subtitle.$t");
            SubTitle = sum != null ? (sum.Value<string>() ?? string.Empty) : string.Empty;

            var link = jsvideo.SelectToken("feed.link[1].href");
            Link = link != null ? string.Format("{0}&alt=json", link.Value<string>()) : string.Empty;

            var tpid = jsvideo.SelectToken("feed.author[0].uri.$t");
            ChannelID = tpid != null ? tpid.Value<string>().Split('/').Last() ?? string.Empty : string.Empty;
        }
    }
}
