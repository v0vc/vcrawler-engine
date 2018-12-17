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

        CookieContainer ChannelCookies { get; set; }
        ObservableCollection<IVideoItem> ChannelItems { get; set; }
        ICollectionView ChannelItemsCollectionView { get; set; }
        int ChannelItemsCount { get; set; }
        long ChannelViewCount { get; set; }
        long ChannelLikeCount { get; set; }
        long ChannelDislikeCount { get; set; }
        long ChannelCommentCount { get; set; }
        ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        ChannelState ChannelState { get; set; }
        ObservableCollection<ITag> ChannelTags { get; set; }
        IEnumerable<string> Statistics { get; }
        string SelectedStat { get; set; }
        int CountNew { get; set; }
        string DirPath { get; set; }
        string FilterVideoKey { get; set; }
        string ID { get; set; }
        bool IsHasNewFromSync { get; set; }
        bool IsShowSynced { get; set; }
        bool Loaded { get; set; }
        int PlaylistCount { get; set; }
        IVideoItem SelectedItem { get; set; }
        SiteType Site { get; }
        string ChannelStatistics { get; set; }
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
