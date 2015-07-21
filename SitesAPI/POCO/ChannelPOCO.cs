using System.Threading.Tasks;
using Interfaces.POCO;
using Newtonsoft.Json.Linq;

namespace SitesAPI.POCO
{
    public class ChannelPOCO : IChannelPOCO
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; private set; }
        public byte[] Thumbnail { get; set; }
        public string Site { get; set; }

        public static async Task<ChannelPOCO> CreatePoco(string id, JObject record)
        {
            var ch = new ChannelPOCO {ID = id};

            var ttitle = record.SelectToken("items[0].snippet.title");
            ch.Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var sub = record.SelectToken("items[0].snippet.description");
            ch.SubTitle = sub != null ? (sub.Value<string>() ?? string.Empty) : string.Empty;

            var link = record.SelectToken("items[0].snippet.thumbnails.default.url");
            if (link != null)
            {
                ch.Thumbnail = await SiteHelper.GetStreamFromUrl(link.Value<string>());
            }

            return ch;
        }
    }
}
