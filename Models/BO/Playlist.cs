// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task DeletePlaylistAsync()
        {
            // await ((PlaylistFactory) ServiceLocator.PlaylistFactory).DeletePlaylistAsync(ID);
            await PlaylistFactory.DeletePlaylistAsync(ID);
        }

        public Task DownloadPlaylist()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IVideoItem>> GetPlaylistItemsDbAsync()
        {
            return await PlaylistFactory.GetPlaylistItemsDbAsync(ID, ChannelId);
        }

        public async Task<IEnumerable<string>> GetPlaylistItemsIdsListDbAsync()
        {
            return await PlaylistFactory.GetPlaylistItemsIdsListDbAsync(ID);
        }

        public async Task<IEnumerable<string>> GetPlaylistItemsIdsListNetAsync(int maxResult)
        {
            return await PlaylistFactory.GetPlaylistItemsIdsListNetAsync(ID, maxResult);
        }

        public async Task<IEnumerable<IVideoItem>> GetPlaylistItemsNetAsync()
        {
            return await PlaylistFactory.GetPlaylistItemsNetAsync(this);
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
