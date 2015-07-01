using System;
using System.Data;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class VideoItemPOCO : IVideoItemPOCO
    {
        public string ID { get; set; }

        public string ParentID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public long ViewCount { get; set; }

        public int Duration { get; set; }

        public int Comments { get; set; }

        public byte[] Thumbnail { get; set; }

        public DateTime Timestamp { get; set; }
        public string Status { get; set; }

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
