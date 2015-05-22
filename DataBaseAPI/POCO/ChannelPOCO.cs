using System;
using System.Data;
using Interfaces.POCO;


namespace DataBaseAPI.POCO
{
    public class ChannelPOCO : IChannelPOCO
    {
        public string ID { get; set; }

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public DateTime LastUpdated { get; set; }

        public byte[] Thumbnail { get; set; }

        public string Site { get; set; }

        public ChannelPOCO(IDataRecord reader)
        {
            ID = reader[SqLiteDatabase.ChannelId] as string;
            Title = reader[SqLiteDatabase.ChannelTitle] as string;
            SubTitle = reader[SqLiteDatabase.ChannelSubTitle] as string;
            Thumbnail = reader[SqLiteDatabase.ChannelThumbnail] as byte[];
            Site = reader[SqLiteDatabase.ChannelSite] as string;
        }

        //internal static byte[] GetBytes(IDataRecord reader)
        //{
        //    const int chunkSize = 2 * 1024;
        //    var buffer = new byte[chunkSize];
        //    long fieldOffset = 0;
        //    using (var stream = new MemoryStream())
        //    {
        //        long bytesRead;
        //        var col = reader.GetOrdinal(SqLiteDatabase.ChannelThumbnail);
        //        while ((bytesRead = reader.GetBytes(col, fieldOffset, buffer, 0, buffer.Length)) > 0)
        //        {
        //            stream.Write(buffer, 0, (int)bytesRead);
        //            stream.Flush();
        //            fieldOffset += bytesRead;
        //        }
        //        return stream.ToArray();
        //    }
        //}
    }
}
