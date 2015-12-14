// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        ICollectionView ChannelItemsCollectionView { get; set; }
        int ChannelItemsCount { get; set; }
        ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        ObservableCollection<ITag> ChannelTags { get; set; }
        int CountNew { get; set; }
        string FilterVideoKey { get; set; }
        string ID { get; set; }
        ChannelState ChannelState { get; set; }
        bool IsInWork { get; set; }
        int PlaylistCount { get; set; }
        IVideoItem SelectedItem { get; set; }
        IList<IVideoItem> SelectedItems { get; }
        SiteType Site { get; set; }
        string SiteAdress { get; set; }
        string SubTitle { get; set; }
        byte[] Thumbnail { get; set; }
        string Title { get; set; }
        List<string> AddedIds { get; }
        List<IVideoItem> DeletedIds { get; }
        bool IsShowSynced { get; set; }

        #endregion

        #region Methods

        void AddNewItem(IVideoItem item, SyncState syncState);

        #endregion
    }
}
