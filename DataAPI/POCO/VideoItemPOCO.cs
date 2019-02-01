// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
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
            long viewcount,
            int duration,
            long comments,
            byte[] thumbnail,
            DateTime timestamp,
            byte syncstate,
            byte watchstate,
            long likes,
            long dislikes,
            long viewdiff,
            string pname)
        {
            ParentName = pname;
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
            LikeCount = likes;
            DislikeCount = dislikes;
            ViewDiff = viewdiff;
        }

        public VideoItemPOCO(string id = null)
        {
            if (id != null)
            {
                ID = id;
            }
        }

        #endregion

        #region Properties

        public long Comments { get; set; }
        public string Description { get; set; }
        public long DislikeCount { get; set; }
        public int Duration { get; set; }
        public string ID { get; set; }
        public long LikeCount { get; set; }
        public string ParentID { get; set; }
        public string ParentName { get; set; }
        public PrivacyStatus Status { get; set; }
        public byte SyncState { get; private set; }
        public byte[] Thumbnail { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public long ViewCount { get; set; }
        public long ViewDiff { get; set; }
        public byte WatchState { get; private set; }

        #endregion
    }
}
