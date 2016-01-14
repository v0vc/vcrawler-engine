// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Interfaces.Enums;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO.Channels
{
    public sealed class YouChannel : IChannel, INotifyPropertyChanged
    {
        #region Fields

        private List<string> added;
        private int channelItemsCount;
        private ChannelState channelState;
        private int countNew;
        private List<string> deleted;
        private string filterVideoKey;
        private bool isShowSynced;
        private int playlistCount;
        private IVideoItem selectedItem;
        private string subTitle;
        private string title;

        #endregion

        #region Constructors

        public YouChannel()
        {
            ChannelItems = new ObservableCollection<IVideoItem>();
            ChannelPlaylists = new ObservableCollection<IPlaylist>();
            ChannelTags = new ObservableCollection<ITag>();
            ChannelCookies = new CookieContainer();
        }

        #endregion

        #region Static Methods

        public static string MakePlaylistUploadId(string id)
        {
            var sb = new StringBuilder(id);
            sb[1] = 'U';
            return sb.ToString();
        }

        #endregion

        #region Methods

        public async void RestoreFullChannelItems(string dirPath)
        {
            if (ChannelItemsCount <= ChannelItems.Count)
            {
                return;
            }
            await ChannelFactory.FillChannelItemsFromDbAsync(this, dirPath, ChannelItemsCount - ChannelItems.Count, ChannelItems.Count);
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
            if (value == null || value.Title == null)
            {
                return false;
            }

            return value.Title.ToLower().Contains(FilterVideoKey.ToLower());
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
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
                channelItemsCount = value;
                OnPropertyChanged();
            }
        }

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

        public int CountNew
        {
            get
            {
                return countNew;
            }
            set
            {
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
                RestoreFullChannelItems(DirPath);
                ChannelItemsCollectionView.Filter = FilterVideoByTitle;
                OnPropertyChanged();
            }
        }

        public string ID { get; set; }

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
                    RestoreFullChannelItems(DirPath);
                    ChannelItemsCollectionView.Filter = FilterVideoBySynced;
                }
                else
                {
                    ChannelItemsCollectionView.Filter = null;
                }
                OnPropertyChanged();
            }
        }

        public int PlaylistCount
        {
            get
            {
                return playlistCount;
            }
            set
            {
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

        public SiteType Site { get; set; }

        public string SubTitle
        {
            get
            {
                return subTitle;
            }
            set
            {
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
                title = value;
                OnPropertyChanged();
            }
        }

        public void AddNewItem(IVideoItem item)
        {
            item.Site = Site;
            item.FileState = ItemState.LocalNo;
            item.Site = Site;

            if (item.SyncState == SyncState.Added)
            {
                ChannelItems.Insert(0, item);
                CountNew += 1;
            }
            else
            {
                ChannelItems.Add(item);
            }
            ChannelItemsCount += 1;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
