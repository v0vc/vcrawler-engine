// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAPI.Videos;
using Interfaces.Enums;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO
{
    public class Playlist : IPlaylist
    {
        #region IPlaylist Members

        public string ChannelId { get; set; }
        public string ID { get; set; }
        public List<string> PlItems { get; set; }
        public SiteType Site { get; set; }
        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Title { get; set; }

        public Task DownloadPlaylist()
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetPlaylistItemsIdsListNetAsync(int maxResult)
        {
            return await YouTubeSite.GetPlaylistItemsIdsListNetAsync(ID, maxResult);
        }

        public async Task InsertPlaylistAsync()
        {
            await PlaylistFactory.InsertPlaylistAsync(this);
        }

        public async Task UpdatePlaylistAsync(string videoId)
        {
            await PlaylistFactory.UpdatePlaylistAsync(ID, videoId, ChannelId);
        }

        #endregion
    }
}
