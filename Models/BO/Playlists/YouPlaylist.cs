// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using Interfaces.Enums;
using Interfaces.Models;

namespace Models.BO.Playlists
{
    public class YouPlaylist : IPlaylist
    {
        #region IPlaylist Members

        public string ChannelId { get; set; }
        public string ID { get; set; }
        public bool IsDefault { get; set; }
        public List<string> PlItems { get; set; }

        public SiteType Site => SiteType.YouTube;

        public SyncState State { get; set; }

        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Title { get; set; }
        public WatchState WatchState { get; set; }

        #endregion
    }
}
