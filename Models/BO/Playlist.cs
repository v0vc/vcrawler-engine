﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.Factories;

namespace Models.BO
{
    public class Playlist : IPlaylist
    {
        private readonly PlaylistFactory _pf;

        public string ID { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string ChannelId { get; set; }
        public List<IVideoItem> PlaylistItems { get; set; }

        public Playlist(IPlaylistFactory pf)
        {
            _pf = pf as PlaylistFactory;
            PlaylistItems = new List<IVideoItem>();
        }

        public Playlist(IPlaylistPOCO playlist, IPlaylistFactory pf)
        {
            _pf = pf as PlaylistFactory;
            ID = playlist.ID;
            Title = playlist.Title;
            SubTitle = playlist.SubTitle;
            Thumbnail = playlist.Thumbnail;
            ChannelId = playlist.ChannelID;
            PlaylistItems = new List<IVideoItem>();
        }

        public async Task DeletePlaylistAsync()
        {
            await _pf.DeletePlaylistAsync(ID);
            //await ((PlaylistFactory) ServiceLocator.PlaylistFactory).DeletePlaylistAsync(ID);
        }

        public async Task InsertPlaylistAsync()
        {
            await _pf.InsertPlaylistAsync(this);
            //await ((PlaylistFactory)ServiceLocator.PlaylistFactory).InsertPlaylistAsync(this);
        }


        public async Task<List<IVideoItem>> GetPlaylistItemsNetAsync()
        {
            return await _pf.GetPlaylistItemsNetAsync(this);
            //return await ((PlaylistFactory) ServiceLocator.PlaylistFactory).GetPlaylistItemsNetAsync(this);
        }

        public async Task<List<string>> GetPlaylistItemsIdsListNetAsync()
        {
            return await _pf.GetPlaylistItemsIdsListNetAsync(ID);
        }

        public async Task<List<string>> GetPlaylistItemsIdsListDbAsync()
        {
            return await _pf.GetPlaylistItemsIdsListDbAsync(ID);
        }

        public async Task<List<IVideoItem>> GetPlaylistItemsDbAsync()
        {
            return await _pf.GetPlaylistItemsDbAsync(ID, ChannelId);
            //return await ((PlaylistFactory) ServiceLocator.PlaylistFactory).GetPlaylistItemsDbAsync(ID, ChannelId);
        }

        public async Task UpdatePlaylistAsync(string videoId)
        {
            await _pf.UpdatePlaylistAsync(ID, videoId, ChannelId);
            //await ((VideoItemFactory) ServiceLocator.VideoItemFactory).UpdatePlaylistAsync(playlist.ID, ID, ParentID);
        }
    }
}
