using System;
using System.Data;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class ChannelPOCO : IChannelPOCO
    {
        public ChannelPOCO(IDataRecord reader)
        {
            ID = reader[SqLiteDatabase.ChannelId] as string;
            Title = reader[SqLiteDatabase.ChannelTitle] as string;
            SubTitle = reader[SqLiteDatabase.ChannelSubTitle] as string;
            Thumbnail = reader[SqLiteDatabase.ChannelThumbnail] as byte[];
            Site = reader[SqLiteDatabase.ChannelSite] as string;
        }

        public DateTime LastUpdated { get; set; }
        public string ID { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Site { get; set; }
    }
}
