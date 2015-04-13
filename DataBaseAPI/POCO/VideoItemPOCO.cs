using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
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

        public VideoItemPOCO()
        {
            
        }

        public VideoItemPOCO(IDataRecord reader)
        {
            ID = reader[SqLiteDatabase.ItemId] as string;
            ParentID = reader[SqLiteDatabase.ParentID] as string;
            Title = reader[SqLiteDatabase.Title] as string;
            Description = reader[SqLiteDatabase.Description] as string;
            ViewCount = Convert.ToInt32(reader[SqLiteDatabase.ViewCount]);
            Duration = Convert.ToInt32(reader[SqLiteDatabase.Duration]);
            Comments = Convert.ToInt32(reader[SqLiteDatabase.Comments]);
            Thumbnail = (byte[]) reader[SqLiteDatabase.Thumbnail];
            Timestamp = (DateTime) reader[SqLiteDatabase.Timestamp];
        }
    }
}
