// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;
using Interfaces.Enums;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO.Channels
{
    public sealed class YouChannel : CommonChannel, IChannel, INotifyPropertyChanged
    {
        #region Fields

        private List<string> added;
        private int channelItemsCount;
        private ChannelState channelState;
        private int countNew;
        private List<string> deleted;
        private string filterVideoKey;
        private bool isHasScrolled;
        private bool isShowSynced;
        private int playlistCount;
        private IVideoItem selectedItem;
        private string subTitle;
        private string title;
        private string channelStatistics;
        private string selectedStat;

        #endregion

        #region Constructors

        public YouChannel()
        {
            ChannelItems = new ObservableCollection<IVideoItem>();
            ChannelPlaylists = new ObservableCollection<IPlaylist>();
            ChannelTags = new ObservableCollection<ITag>();
            VideoTags = new ObservableCollection<ITag>();
            ChannelCookies = new CookieContainer();
            ChannelItemsCollectionView = CollectionViewSource.GetDefaultView(ChannelItems);
        }

        #endregion

        #region Static Methods

        public static string MakePlaylistUploadId(string id)
        {
            return new StringBuilder(id) { [1] = 'U' }.ToString();
        }

        #endregion

        #region Methods

        public void RestoreFullChannelItems()
        {
            if (isHasScrolled)
            {
                return;
            }
            if (ChannelItemsCount != 0 && ChannelItemsCount <= ChannelItems.Count)
            {
                return;
            }
            isHasScrolled = true;
            ChannelFactory.SetChannelCountAsync(this);
            ChannelFactory.FillChannelItemsFromDbAsync(this, 0, ChannelItems.Select(x => x.ID).ToList());
        }

        private bool FilterVideoBySynced(object item)
        {
            if (added == null)
            {
                added = ChannelItems.Where(x => x.SyncState == SyncState.Added).Select(x => x.ID).ToList();
            }
            if (deleted == null)
            {
                deleted = ChannelItems.Where(x => x.SyncState == SyncState.Deleted).Select(x => x.ID).ToList();
            }
            if (!added.Any() && !deleted.Any())
            {
                return true;
            }
            var value = (IVideoItem)item;
            if (value == null)
            {
                return false;
            }

            bool res = added.Contains(value.ID) || deleted.Contains(value.ID);
            return res;
        }

        private bool FilterVideoByTitle(object item)
        {
            var value = (IVideoItem)item;
            if (value?.Title == null)
            {
                return false;
            }

            return value.Title.ToLower().Contains(FilterVideoKey.ToLower());
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IChannel Members

        public CookieContainer ChannelCookies { get; set; }
        public ObservableCollection<IVideoItem> ChannelItems { get; set; }
        public ICollectionView ChannelItemsCollectionView { get; set; }
        public int ChannelItemsCount
        {
            get
            {
                return channelItemsCount;
            }
            set
            {
                if (value == channelItemsCount)
                {
                    return;
                }
                channelItemsCount = value;
                OnPropertyChanged();
            }
        }
        public long ChannelViewCount { get; set; }
        public long ChannelLikeCount { get; set; }
        public long ChannelDislikeCount { get; set; }
        public long ChannelCommentCount { get; set; }
        public ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        public ChannelState ChannelState
        {
            get
            {
                return channelState;
            }
            set
            {
                if (value == channelState)
                {
                    return;
                }
                channelState = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<ITag> ChannelTags { get; set; }
        public string SelectedStat
        {
            get
            {
                return selectedStat;
            }
            set
            {
                if (value == selectedStat)
                {
                    return;
                }
                selectedStat = value;
                if (selectedStat == null)
                {
                    return;
                }
                ChannelItemsCollectionView.SortDescriptions.Clear();
                ChannelItemsCollectionView.SortDescriptions.Add(new SortDescription(Stats[selectedStat], ListSortDirection.Descending));
            }
        }
        public int CountNew
        {
            get
            {
                return countNew;
            }
            set
            {
                if (value == countNew)
                {
                    return;
                }
                countNew = value;
                OnPropertyChanged();
            }
        }
        public string DirPath { get; set; }
        public string FilterVideoKey
        {
            get
            {
                return filterVideoKey;
            }
            set
            {
                if (value == filterVideoKey)
                {
                    return;
                }

                filterVideoKey = value;
                RestoreFullChannelItems();
                ChannelItemsCollectionView.Filter = FilterVideoByTitle;
            }
        }
        public string ID { get; set; }
        public bool IsAllItems { get; set; }
        public bool IsHasNewFromSync { get; set; }
        public bool IsShowSynced
        {
            get
            {
                return isShowSynced;
            }
            set
            {
                if (value == isShowSynced)
                {
                    return;
                }
                isShowSynced = value;

                if (isShowSynced)
                {
                    RestoreFullChannelItems();
                    ChannelItemsCollectionView.Filter = FilterVideoBySynced;
                }
                else
                {
                    ChannelItemsCollectionView.Filter = null;
                }
            }
        }
        public bool Loaded { get; set; }
        public int PlaylistCount
        {
            get
            {
                return playlistCount;
            }
            set
            {
                if (value == playlistCount)
                {
                    return;
                }
                playlistCount = value;
                OnPropertyChanged();
            }
        }
        public IVideoItem SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                if (value == selectedItem)
                {
                    return;
                }
                selectedItem = value;
                OnPropertyChanged();
            }
        }
        public SiteType Site => SiteType.YouTube;
        public string ChannelStatistics
        {
            get
            {
                return channelStatistics;
            }
            set
            {
                channelStatistics = value;
                OnPropertyChanged();
            }
        }
        public string SubTitle
        {
            get
            {
                return subTitle;
            }
            set
            {
                if (value == subTitle)
                {
                    return;
                }
                subTitle = value;
                OnPropertyChanged();
            }
        }
        public byte[] Thumbnail { get; set; }
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                if (value == title)
                {
                    return;
                }
                title = value;
                OnPropertyChanged();
            }
        }
        public bool UseFast { get; set; }
        public ObservableCollection<ITag> VideoTags { get; set; }
        public void AddNewItem(IVideoItem item, bool isIncrease = true, bool isUpdateCount = true)
        {
            if (item == null)
            {
                throw new ArgumentException("item");
            }

            item.ParentTitle = Title;
            if (item.SyncState == SyncState.Added)
            {
                ChannelItems.Insert(0, item);
                if (isIncrease)
                {
                    CountNew += 1;
                }
            }
            else
            {
                ChannelItems.Add(item);
            }
            if (isUpdateCount)
            {
                ChannelItemsCount += 1;
            }
        }
        public void DeleteItem(IVideoItem item)
        {
            if (item == null)
            {
                throw new ArgumentException("item");
            }

            ChannelItems.Remove(item);

            ChannelItemsCount -= 1;

            if (item.SyncState == SyncState.Added)
            {
                CountNew -= 1;
            }
        }
        public void RefreshView(string field)
        {
            ChannelItemsCollectionView.SortDescriptions.Clear();
            ChannelItemsCollectionView.SortDescriptions.Add(new SortDescription(field, ListSortDirection.Descending));
            //ChannelItemsCollectionView.Refresh();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
