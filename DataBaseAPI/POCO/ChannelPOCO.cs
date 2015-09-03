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
            // SubTitle = reader[SqLiteDatabase.ChannelSubTitle] as string;
            Thumbnail = reader[SqLiteDatabase.ChannelThumbnail] as byte[];
            Site = reader[SqLiteDatabase.ChannelSite] as string;
        }

        public DateTime LastUpdated { get; set; }
        public string ID { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public byte[] Thumbnail { get; private set; }
        public string Site { get; private set; }
    }
}
