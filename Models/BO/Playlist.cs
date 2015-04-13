using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Models;
using Interfaces.POCO;
using Models.Factories;

namespace Models.BO
{
    public class Playlist : IPlaylist
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Link { get; set; }
        public string ChannelId { get; set; }
        public List<IVideoItem> PlaylistItems { get; set; }

        public Playlist()
        {
            PlaylistItems = new List<IVideoItem>();
        }

        public Playlist(IPlaylistPOCO playlist)
        {
            ID = playlist.ID;
            Title = playlist.Title;
            SubTitle = playlist.SubTitle;
            Link = playlist.Link;
            ChannelId = playlist.ChannelID;
            PlaylistItems = new List<IVideoItem>();
        }

        public async Task DeletePlaylistAsync()
        {
            await ((PlaylistFactory) ServiceLocator.PlaylistFactory).DeletePlaylistAsync(ID);
        }

        public async Task InsertPlaylistAsync()
        {
            await ((PlaylistFactory)ServiceLocator.PlaylistFactory).InsertPlaylistAsync(this);
        }


        public async Task<List<IVideoItem>> GetPlaylistItemsNetAsync()
        {
            return await ((PlaylistFactory) ServiceLocator.PlaylistFactory).GetPlaylistItemsNetAsync(this);
        }

        public async Task<List<IVideoItem>> GetPlaylistItemsDbAsync()
        {
            return await ((PlaylistFactory) ServiceLocator.PlaylistFactory).GetPlaylistItemsDbAsync(ID, ChannelId);
        }
    }
}
