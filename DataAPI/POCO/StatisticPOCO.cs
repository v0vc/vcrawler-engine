// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

namespace DataAPI.POCO
{
    public struct StatisticPOCO
    {
        #region Properties

        public long CommentCount { get; set; }
        public long DislikeCount { get; set; }
        public long LikeCount { get; set; }
        public string VideoId { get; set; }
        public long ViewCount { get; set; }
        public bool Filled => ViewCount > 0 && LikeCount > 0;

        #endregion
    }
}
