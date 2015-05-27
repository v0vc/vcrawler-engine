﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface IChannel
    {
        string ID { get; set; }

        string Title { get; set; }

        string SubTitle { get; set; }

        byte[] Thumbnail { get; set; }

        string Site { get; set; }

        ObservableCollection<IVideoItem> ChannelItems { get; set; }

        ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }

        List<ITag> Tags { get; set; }

        int CountNew { get; set; }

        Task<List<IVideoItem>> GetChannelItemsDbAsync();

        Task SyncChannelAsync(bool isSyncPls);

        Task<List<IVideoItem>> GetChannelItemsNetAsync(int maxresult);

        Task<List<IVideoItem>> GetPopularItemsNetAsync(string regionID, int maxresult);

        Task<List<IVideoItem>> SearchItemsNetAsync(string key, int maxresult);

        Task<List<IPlaylist>> GetChannelPlaylistsNetAsync();

        Task<List<IPlaylist>> GetChannelPlaylistsAsync();

        Task<int> GetChannelItemsCountDbAsync();

        Task<int> GetChannelItemsCountNetAsync();

        Task<List<string>> GetChannelItemsIdsListNetAsync(int maxresult);

        Task<List<string>> GetChannelItemsIdsListDbAsync();

        Task FillChannelItemsDbAsync();

        Task InsertChannelAsync();

        Task DeleteChannelAsync();

        Task RenameChannelAsync(string newName);

        Task InsertChannelItemsAsync();

        Task<List<ITag>> GetChannelTagsAsync();

        Task InsertChannelTagAsync(string tag);

        Task DeleteChannelTagAsync(string tag);
    }
}