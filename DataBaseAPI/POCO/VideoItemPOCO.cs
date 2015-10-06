// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Data;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class VideoItemPOCO : IVideoItemPOCO
    {
        #region Constructors

        public VideoItemPOCO(IDataRecord reader)
        {
            ID = reader[SqLiteDatabase.ItemId] as string;
            ParentID = reader[SqLiteDatabase.ParentID] as string;
            Title = reader[SqLiteDatabase.Title] as string;

            // Description = reader[SqLiteDatabase.Description] as string;
            ViewCount = Convert.ToInt32(reader[SqLiteDatabase.ViewCount]);
            Duration = Convert.ToInt32(reader[SqLiteDatabase.Duration]);
            Comments = Convert.ToInt32(reader[SqLiteDatabase.Comments]);
            Thumbnail = (byte[])reader[SqLiteDatabase.Thumbnail];
            Timestamp = (DateTime)reader[SqLiteDatabase.Timestamp];
        }

        #endregion

        #region IVideoItemPOCO Members

        public int Comments { get; private set; }
        public string Description { get; private set; }
        public int Duration { get; private set; }
        public string ID { get; private set; }
        public string ParentID { get; private set; }
        public string Status { get; set; }
        public byte[] Thumbnail { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Title { get; private set; }
        public long ViewCount { get; private set; }

        #endregion
    }
}
