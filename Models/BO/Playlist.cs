// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

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

        private readonly PlaylistFactory _pf;

        #endregion

        #region Constructors

        public Playlist(PlaylistFactory pf)
        {
            _pf = pf;
        }

        private Playlist()
        {
        }

        #endregion

        #region IPlaylist Members

        public string ChannelId { get; set; }
        public string ID { get; set; }
        public List<IVideoItem> PlaylistItems { get; set; }
        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Title { get; set; }
        public SiteType Site { get; set; }

        public async Task DeletePlaylistAsync()
        {
            // await ((PlaylistFactory) ServiceLocator.PlaylistFactory).DeletePlaylistAsync(ID);
            await _pf.DeletePlaylistAsync(ID);
        }

        public async Task<IEnumerable<IVideoItem>> GetPlaylistItemsDbAsync()
        {
            return await _pf.GetPlaylistItemsDbAsync(ID, ChannelId);
        }

        public async Task<IEnumerable<string>> GetPlaylistItemsIdsListDbAsync()
        {
            return await _pf.GetPlaylistItemsIdsListDbAsync(ID);
        }

        public async Task<IEnumerable<string>> GetPlaylistItemsIdsListNetAsync()
        {
            return await _pf.GetPlaylistItemsIdsListNetAsync(ID);
        }

        public async Task<IEnumerable<IVideoItem>> GetPlaylistItemsNetAsync()
        {
            return await _pf.GetPlaylistItemsNetAsync(this);
        }

        public async Task InsertPlaylistAsync()
        {
            await _pf.InsertPlaylistAsync(this);
        }

        public async Task UpdatePlaylistAsync(string videoId)
        {
            await _pf.UpdatePlaylistAsync(ID, videoId, ChannelId);
        }

        #endregion
    }
}
