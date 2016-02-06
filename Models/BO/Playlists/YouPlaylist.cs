// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Threading.Tasks;
using Interfaces.Enums;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO.Playlists
{
    public class YouPlaylist : IPlaylist
    {
        #region IPlaylist Members

        public string ChannelId { get; set; }
        public string ID { get; set; }
        public bool IsDefault { get; set; }
        public List<string> PlItems { get; set; }

        public SiteType Site
        {
            get
            {
                return SiteType.YouTube;
            }
        }

        public SyncState State { get; set; }

        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Title { get; set; }
        public WatchState WatchState { get; set; }

        public async Task UpdatePlaylistAsync(string videoId)
        {
            await PlaylistFactory.UpdatePlaylistAsync(ID, videoId, ChannelId);
        }

        #endregion
    }
}
