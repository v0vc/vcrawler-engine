// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using Interfaces.Enums;

namespace Interfaces.Models
{
    public interface IChannel
    {
        #region Properties

        CookieContainer ChannelCookies { get; set; }
        ObservableCollection<IVideoItem> ChannelItems { get; set; }
        ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        ObservableCollection<ITag> ChannelTags { get; set; }
        int CountNew { get; set; }
        string ID { get; set; }
        bool IsDownloading { get; set; }
        bool IsInWork { get; set; }
        bool IsShowRow { get; set; }
        bool IsSelected { get; set; }
        int PlaylistCount { get; set; }
        string SiteAdress { get; set; }
        string SubTitle { get; set; }
        byte[] Thumbnail { get; set; }
        string Title { get; set; }
        int ChannelItemsCount { get; set; }
        SiteType Site { get; set; }

        IList<IVideoItem> SelectedItems { get; }
        #endregion

        #region Methods

        void AddNewItem(IVideoItem item, bool isNew);

        Task DeleteChannelAsync();

        Task DeleteChannelTagAsync(string tag);

        void FillChannelCookieDb();

        Task FillChannelCookieNetAsync();

        Task FillChannelDescriptionAsync();

        Task FillChannelItemsDbAsync(string dir, int count, int offset);

        Task<int> GetChannelItemsCountDbAsync();

        Task<int> GetChannelItemsCountNetAsync();

        Task<IEnumerable<IVideoItem>> GetChannelItemsDbAsync(int count, int offset);

        Task<IEnumerable<IChannel>> GetRelatedChannelNetAsync(string id, SiteType site);

        Task<IEnumerable<string>> GetChannelItemsIdsListDbAsync();

        Task<IEnumerable<string>> GetChannelItemsIdsListNetAsync(int maxresult);

        Task<IEnumerable<IVideoItem>> GetChannelItemsNetAsync(int maxresult);

        Task<int> GetChannelPlaylistCountDbAsync();

        Task<IEnumerable<IPlaylist>> GetChannelPlaylistsDbAsync();

        Task<IEnumerable<IPlaylist>> GetChannelPlaylistsNetAsync();

        Task<IEnumerable<ITag>> GetChannelTagsAsync();

        Task<IEnumerable<IVideoItem>> GetPopularItemsNetAsync(string regionID, int maxresult);

        Task InsertChannelAsync();

        Task InsertChannelItemsAsync();

        Task InsertChannelTagAsync(string tag);

        Task RenameChannelAsync(string newName);

        Task<IEnumerable<IVideoItem>> SearchItemsNetAsync(string key, string region, int maxresult);

        void StoreCookies();

        Task SyncChannelAsync(bool isSyncPls);

        Task SyncChannelPlaylistsAsync();

        Task<ICred> GetChannelCredentialsAsync();

        #endregion
    }
}
