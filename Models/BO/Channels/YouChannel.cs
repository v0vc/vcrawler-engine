// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Interfaces.Enums;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO.Channels
{
    public sealed class YouChannel : IChannel, INotifyPropertyChanged
    {
        #region Static and Readonly Fields

        private readonly ChannelFactory channelFactory;

        #endregion

        #region Fields

        private List<string> added;
        private int channelItemsCount;
        private ChannelState channelState;
        private int countNew;
        private List<string> deleted;
        private string filterVideoKey;
        private bool isInWork;
        private bool isShowSynced;
        private int playlistCount;
        private IVideoItem selectedItem;
        private string subTitle;
        private string title;

        #endregion

        #region Constructors

        public YouChannel(ChannelFactory channelFactory)
        {
            this.channelFactory = channelFactory;
            ChannelItems = new ObservableCollection<IVideoItem>();
            ChannelPlaylists = new ObservableCollection<IPlaylist>();
            ChannelTags = new ObservableCollection<ITag>();
            ChannelCookies = new CookieContainer();
        }

        private YouChannel()
        {
        }

        #endregion

        #region Methods

        public async Task DeleteChannelPlaylistsAsync()
        {
            await channelFactory.DeleteChannelPlaylistsAsync(ID);
        }

        public async Task DeleteChannelTagAsync(string tag)
        {
            await channelFactory.DeleteChannelTagAsync(ID, tag);
        }

        public void FillChannelCookieDb()
        {
            channelFactory.FillChannelCookieDb(this);
        }

        public async Task FillChannelCookieNetAsync()
        {
            await channelFactory.FillChannelCookieNetAsync(this);
        }

        public async Task FillChannelDescriptionAsync()
        {
            await channelFactory.FillChannelDescriptionAsync(this);
        }

        public async Task<IEnumerable<IVideoItem>> GetChannelItemsNetAsync(int maxresult)
        {
            return await channelFactory.GetChannelItemsNetAsync(this, maxresult);
        }

        public async Task<IEnumerable<IPlaylist>> GetChannelPlaylistsDbAsync()
        {
            return await channelFactory.GetChannelPlaylistsAsync(ID);
        }

        public async Task<IEnumerable<IPlaylist>> GetChannelPlaylistsNetAsync()
        {
            return await channelFactory.GetChannelPlaylistsNetAsync(ID);
        }

        public async Task<IEnumerable<ITag>> GetChannelTagsAsync()
        {
            return await channelFactory.GetChannelTagsAsync(ID);
        }

        public async Task<IEnumerable<IChannel>> GetRelatedChannelNetAsync(string id, SiteType site)
        {
            return await channelFactory.GetRelatedChannelNetAsync(this);
        }

        public async Task InsertChannelItemsAsync()
        {
            await channelFactory.InsertChannelItemsAsync(this);
        }

        public async Task InsertChannelTagAsync(string tag)
        {
            await channelFactory.InsertChannelTagAsync(ID, tag);
        }

        public async Task RenameChannelAsync(string newName)
        {
            await channelFactory.RenameChannelAsync(ID, newName);
        }

        public async void RestoreFullChannelItems(string dirPath)
        {
            if (ChannelItemsCount <= ChannelItems.Count)
            {
                return;
            }
            await FillChannelItemsDbAsync(dirPath, ChannelItemsCount - ChannelItems.Count, ChannelItems.Count);
        }

        public async Task SyncChannelPlaylistsAsync()
        {
            await channelFactory.SyncChannelPlaylistsAsync(this);
        }

        private async Task FillChannelItemsDbAsync(string dir, int count, int offset)
        {
            await channelFactory.FillChannelItemsFromDbAsync(this, dir, count, offset);
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

        public bool IsInWork
        {
            get
            {
                return isInWork;
            }
            set
            {
                isInWork = value;
                OnPropertyChanged();
            }
        }

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

        public IList<IVideoItem> SelectedItems
        {
            get
            {
                return ChannelItems.Where(x => x.IsSelected).ToList();
            }
        }

        public SiteType Site { get; set; }
        public string SiteAdress { get; set; }

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
            item.IsHasLocalFile = false;
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
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
