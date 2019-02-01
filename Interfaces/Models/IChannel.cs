// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using Interfaces.Enums;

namespace Interfaces.Models
{
    public interface IChannel
    {
        #region Properties

        long ChannelCommentCount { get; set; }

        CookieContainer ChannelCookies { get; set; }
        long ChannelDislikeCount { get; set; }
        ObservableCollection<IVideoItem> ChannelItems { get; set; }
        ICollectionView ChannelItemsCollectionView { get; set; }
        int ChannelItemsCount { get; set; }
        long ChannelLikeCount { get; set; }
        ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        ChannelState ChannelState { get; set; }
        string ChannelStatistics { get; set; }
        ObservableCollection<ITag> ChannelTags { get; set; }
        long ChannelViewCount { get; set; }
        int CountNew { get; set; }
        string DirPath { get; set; }
        string FilterVideoKey { get; set; }
        string ID { get; set; }
        bool IsAllItems { get; set; }
        bool IsHasNewFromSync { get; set; }
        bool IsShowSynced { get; set; }
        bool Loaded { get; set; }
        int PlaylistCount { get; set; }
        IVideoItem SelectedItem { get; set; }
        string SelectedStat { get; set; }
        SiteType Site { get; }
        IEnumerable<string> Statistics { get; }
        string SubTitle { get; set; }
        byte[] Thumbnail { get; set; }
        string Title { get; set; }
        bool UseFast { get; set; }
        ObservableCollection<ITag> VideoTags { get; set; }

        #endregion

        #region Methods

        void AddNewItem(IVideoItem item, bool isIncrease = true, bool isUpdateCount = true);

        void DeleteItem(IVideoItem item);

        void RefreshView(string field);

        #endregion
    }
}
