using System.Threading.Tasks;
using Interfaces.POCO;
using Newtonsoft.Json.Linq;

namespace SitesAPI.POCO
{
    public class PlaylistPOCO : IPlaylistPOCO
    {
        public string ID { get; set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public byte[] Thumbnail { get; private set; }
        public string ChannelID { get; set; }

        public async Task FillFieldsFromGetting(JToken record)
        {
            var tid = record.SelectToken("id");
            ID = tid != null ? (tid.Value<string>() ?? string.Empty) : string.Empty;

            var ttitle = record.SelectToken("snippet.title");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var desc = record.SelectToken("snippet.description");
            SubTitle = desc != null ? (desc.Value<string>() ?? string.Empty) : string.Empty;

            var link = record.SelectToken("snippet.thumbnails.default.url");
            if (link != null)
            {
                Thumbnail = await SiteHelper.GetStreamFromUrl(link.Value<string>());
            }
        }

        public async Task FillFieldsFromSingle(JObject record)
        {
            var ttitle = record.SelectToken("items[0].snippet.title");
            Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var sum = record.SelectToken("items[0].snippet.description");
            SubTitle = sum != null ? (sum.Value<string>() ?? string.Empty) : string.Empty;

            var tpid = record.SelectToken("items[0].snippet.channelId");
            ChannelID = tpid != null ? tpid.Value<string>() ?? string.Empty : string.Empty;

            var link = record.SelectToken("items[0].snippet.thumbnails.default.url");
            if (link != null)
            {
                Thumbnail = await SiteHelper.GetStreamFromUrl(link.Value<string>());
            }
        }
    }
}
