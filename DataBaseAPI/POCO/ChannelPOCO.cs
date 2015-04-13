using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Interfaces.POCO;


namespace DataBaseAPI.POCO
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

        public ChannelPOCO()
        {
            
        }
        public ChannelPOCO(IDataRecord reader)
        {
            ID = reader[SqLiteDatabase.ChannelId] as string;
            Title = reader[SqLiteDatabase.ChannelTitle] as string;
            SubTitle = reader[SqLiteDatabase.ChannelSubTitle] as string;
            LastUpdated = (DateTime)reader[SqLiteDatabase.ChannelLastUpdated];
            Thumbnail = (byte[]) reader[SqLiteDatabase.ChannelThumbnail];
            //Thumbnail = GetBytes(reader);
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
