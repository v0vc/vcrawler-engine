// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using Interfaces.Enums;

namespace Interfaces.Models
{
    public interface IPlaylist
    {
        #region Properties

        string ChannelId { get; set; }
        string ID { get; set; }
        bool IsDefault { get; }
        List<string> PlItems { get; set; }
        SiteType Site { get; }
        SyncState State { get; set; }
        string SubTitle { get; set; }
        byte[] Thumbnail { get; set; }
        string Title { get; set; }
        WatchState WatchState { get; set; }

        #endregion
    }
}
