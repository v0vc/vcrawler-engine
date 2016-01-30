// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using Interfaces.Enums;

namespace DataAPI.POCO
{
    public class VideoItemPOCO
    {
        #region Constructors

        public VideoItemPOCO(string id, 
            string parentid, 
            string title, 
            int viewcount, 
            int duration, 
            int comments, 
            byte[] thumbnail, 
            DateTime timestamp, 
            byte syncstate,
            byte watchstate)
        {
            ID = id;
            ParentID = parentid;
            Title = title;
            ViewCount = viewcount;
            Duration = duration;
            Comments = comments;
            Thumbnail = thumbnail;
            Timestamp = timestamp;
            SyncState = syncstate;
            WatchState = watchstate;
        }

        public VideoItemPOCO(string id)
        {
            ID = id;
        }

        public VideoItemPOCO()
        {
            
        }

        //public VideoItemPOCO(string id, SiteType site)
        //{
        //    ID = id;
        //    Site = site;
        //}

        //public VideoItemPOCO(SiteType siteType)
        //{
        //    Site = siteType;
        //}

        #endregion

        #region Properties

        public int Comments { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public string ID { get; set; }
        public string ParentID { get; set; }
        //public SiteType Site { get; private set; }
        public PrivacyStatus Status { get; set; }
        public byte SyncState { get; private set; }
        public byte[] Thumbnail { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public long ViewCount { get; set; }
        public byte WatchState { get; private set; }

        #endregion
    }
}
