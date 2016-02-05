// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Interfaces.Enums;
using Interfaces.Models;

namespace Models.BO.Playlists
{
    public class ServicePlaylist : IPlaylist
    {
        #region Constructors

        public ServicePlaylist()
        {
            PlItems = new List<string>();
        }

        #endregion

        #region IPlaylist Members

        public string ChannelId { get; set; }
        public string ID { get; set; }
        public bool IsDefault { get; private set; }
        public List<string> PlItems { get; set; }
        public SiteType Site { get; private set; }
        public SyncState State { get; set; }
        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Title { get; set; }

        public Task UpdatePlaylistAsync(string videoId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
