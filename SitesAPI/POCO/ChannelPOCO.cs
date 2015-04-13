using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Interfaces.POCO;
using Newtonsoft.Json.Linq;


namespace SitesAPI.POCO
{
    [DataContract]
    public class ChannelPOCO : IChannelPOCO
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string SubTitle { get; set; }

        [DataMember]
        public DateTime LastUpdated { get; set; }

        [DataMember]
        public byte[] Thumbnail { get; set; }

        [DataMember]
        public string Site { get; set; }


        public static async Task<ChannelPOCO> CreatePoco(string id, JObject record)
        {
            var ch = new ChannelPOCO {ID = id, Site = "youtube.com"};

            var ttitle = record.SelectToken("entry.title.$t");
            ch.Title = ttitle != null ? (ttitle.Value<string>() ?? string.Empty) : string.Empty;

            var sub = record.SelectToken("entry.summary.$t");
            ch.SubTitle = sub != null ? (sub.Value<string>() ?? string.Empty) : string.Empty;

            var upd = record.SelectToken("entry.updated.$t");
            ch.LastUpdated = upd != null ? (upd.Value<DateTime?>() ?? DateTime.MinValue) : DateTime.MinValue;

            var link = record.SelectToken("entry.media$thumbnail.url");
            if (link != null)
            {
                ch.Thumbnail = await SiteHelper.GetStreamFromUrl(link.Value<string>());
            }

            return ch;
        }
    }
}
