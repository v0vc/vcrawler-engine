using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Models;
using Interfaces.POCO;
using Models.Factories;

namespace Models.BO
{
    public class Channel :IChannel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public DateTime LastUpdated { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Site { get; set; }
        public List<IVideoItem> ChannelItems { get; set; }
        public List<IPlaylist> ChannelPlaylists { get; set; }
        public List<ITag> Tags { get; set; }

        public Channel()
        {
            ChannelItems = new List<IVideoItem>();
            ChannelPlaylists = new List<IPlaylist>();
            Tags = new List<ITag>();
        }

        public Channel(IChannelPOCO channel)
        {
            ID = channel.ID;
            Title = channel.Title;
            SubTitle = channel.SubTitle;
            LastUpdated = channel.LastUpdated;
            Thumbnail = channel.Thumbnail;
            ChannelItems = new List<IVideoItem>();
            ChannelPlaylists = new List<IPlaylist>();
            Tags = new List<ITag>();
        }

        public async Task<List<IVideoItem>> GetChannelItemsAsync()
        {
            return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelItemsAsync(ID);
        }

        public async Task InsertChannelAsync()
        {
            await ((ChannelFactory) ServiceLocator.ChannelFactory).InsertChannelAsync(this);
        }

        public async Task DeleteChannelAsync()
        {
            await ((ChannelFactory) ServiceLocator.ChannelFactory).DeleteChannelAsync(ID);
        }

        public async Task RenameChannelAsync(string newName)
        {
            await ((ChannelFactory) ServiceLocator.ChannelFactory).RenameChannelAsync(ID, newName);
        }

        public async Task InsertChannelItemsAsync()
        {
            await ((ChannelFactory) ServiceLocator.ChannelFactory).InsertChannelItemsAsync(this);
        }

        public async Task<List<IVideoItem>> GetChannelItemsNetAsync()
        {
            return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelItemsNetAsync(ID);
        }

        public async Task<List<IVideoItem>> GetPopularItemsNetAsync(string regionID, int maxresult)
        {
            return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetPopularItemsNetAsync(regionID, maxresult);
        }

        public async Task<List<IVideoItem>> SearchItemsNetAsync(string key, int maxresult)
        {
            return await ((ChannelFactory) ServiceLocator.ChannelFactory).SearchItemsNetAsync(key, maxresult);
        }

        public async Task<List<IPlaylist>> GetChannelPlaylistsNetAsync()
        {
            return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelPlaylistsNetAsync(ID);
        }

        public async Task<List<IPlaylist>> GetChannelPlaylistsAsync()
        {
            return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelPlaylistsAsync(ID);
        }

        public async Task<List<ITag>> GetChannelTagsAsync()
        {
            return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelTagsAsync(ID);
        }


        public async Task InsertChannelTagAsync(string tag)
        {
            await ((ChannelFactory) ServiceLocator.ChannelFactory).InsertChannelTagAsync(ID, tag);
        }

        public async Task DeleteChannelTagAsync(string tag)
        {
            await ((ChannelFactory) ServiceLocator.ChannelFactory).DeleteChannelTagAsync(ID, tag);
        }
    }
}
