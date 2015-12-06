// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
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
        #region Static and Readonly Fields

        private readonly PlaylistFactory playlistFactory;

        #endregion

        #region Constructors

        public Playlist(PlaylistFactory playlistFactory)
        {
            this.playlistFactory = playlistFactory;
        }

        private Playlist()
        {
        }

        #endregion

        #region IPlaylist Members

        public string ChannelId { get; set; }
        public string ID { get; set; }
        public SiteType Site { get; set; }
        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Title { get; set; }

        public async Task DeletePlaylistAsync()
        {
            // await ((PlaylistFactory) ServiceLocator.PlaylistFactory).DeletePlaylistAsync(ID);
            await playlistFactory.DeletePlaylistAsync(ID);
        }

        public Task DownloadPlaylist()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IVideoItem>> GetPlaylistItemsDbAsync()
        {
            return await playlistFactory.GetPlaylistItemsDbAsync(ID, ChannelId);
        }

        public async Task<IEnumerable<string>> GetPlaylistItemsIdsListDbAsync()
        {
            return await playlistFactory.GetPlaylistItemsIdsListDbAsync(ID);
        }

        public async Task<IEnumerable<string>> GetPlaylistItemsIdsListNetAsync()
        {
            return await playlistFactory.GetPlaylistItemsIdsListNetAsync(ID);
        }

        public async Task<IEnumerable<IVideoItem>> GetPlaylistItemsNetAsync()
        {
            return await playlistFactory.GetPlaylistItemsNetAsync(this);
        }

        public async Task InsertPlaylistAsync()
        {
            await playlistFactory.InsertPlaylistAsync(this);
        }

        public async Task UpdatePlaylistAsync(string videoId)
        {
            await playlistFactory.UpdatePlaylistAsync(ID, videoId, ChannelId);
        }

        #endregion
    }
}
