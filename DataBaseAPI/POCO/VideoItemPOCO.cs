using System;
using System.Data;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class VideoItemPOCO : IVideoItemPOCO
    {
        public VideoItemPOCO(IDataRecord reader)
        {
            ID = reader[SqLiteDatabase.ItemId] as string;
            ParentID = reader[SqLiteDatabase.ParentID] as string;
            Title = reader[SqLiteDatabase.Title] as string;
            // Description = reader[SqLiteDatabase.Description] as string;
            ViewCount = Convert.ToInt32(reader[SqLiteDatabase.ViewCount]);
            Duration = Convert.ToInt32(reader[SqLiteDatabase.Duration]);
            Comments = Convert.ToInt32(reader[SqLiteDatabase.Comments]);
            Thumbnail = (byte[]) reader[SqLiteDatabase.Thumbnail];
            Timestamp = (DateTime) reader[SqLiteDatabase.Timestamp];
        }

        public string ID { get; private set; }
        public string ParentID { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public long ViewCount { get; private set; }
        public int Duration { get; private set; }
        public int Comments { get; private set; }
        public byte[] Thumbnail { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Status { get; set; }
    }
}
