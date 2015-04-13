using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface IChannel
    {
        string ID { get; set; }

        string Title { get; set; }

        string SubTitle { get; set; }

        DateTime LastUpdated { get; set; }

        byte[] Thumbnail { get; set; }

        string Site { get; set; }

        List<IVideoItem> ChannelItems { get; set; }

        List<IPlaylist> ChannelPlaylists { get; set; }

        List<ITag> Tags { get; set; }

        Task<List<IVideoItem>> GetChannelItemsAsync();

        Task InsertChannelAsync();

        Task DeleteChannelAsync();

        Task RenameChannelAsync(string newName);

        Task InsertChannelItemsAsync();

        Task<List<IVideoItem>> GetChannelItemsNetAsync();

        Task<List<IVideoItem>> GetPopularItemsNetAsync(string regionID, int maxresult);

        Task<List<IVideoItem>> SearchItemsNetAsync(string key, int maxresult);

        Task<List<IPlaylist>> GetChannelPlaylistsNetAsync();

        Task<List<IPlaylist>> GetChannelPlaylistsAsync();

        Task<List<ITag>> GetChannelTagsAsync();

        Task InsertChannelTagAsync(string tag);

        Task DeleteChannelTagAsync(string tag);
    }
}
