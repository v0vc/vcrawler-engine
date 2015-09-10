// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;

namespace Interfaces.POCO
{
    public interface IVideoItemPOCO
    {
        #region Properties

        int Comments { get; }
        string Description { get; }
        int Duration { get; }
        string ID { get; }
        string ParentID { get; }
        string Status { get; }
        byte[] Thumbnail { get; }
        DateTime Timestamp { get; }
        string Title { get; }
        long ViewCount { get; }

        #endregion
    }
}
