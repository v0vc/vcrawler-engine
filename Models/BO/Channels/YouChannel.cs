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
using System.Threading.Tasks;
using Interfaces.Enums;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO.Channels
{
    public sealed class YouChannel : IChannel, INotifyPropertyChanged
    {
        #region Static and Readonly Fields

        private readonly ChannelFactory cf;

        #endregion

        #region Fields

        private int channelItemsCount;
        private int countNew;
        private string filterVideoKey;
        private bool isDownloading;
        private bool isInWork;
        private int playlistCount;
        private string subTitle;
        private string title;

        #endregion

        #region Constructors

        public YouChannel(ChannelFactory cf)
        {
            this.cf = cf;
        }

        private YouChannel()
        {
        }

        #endregion

        #region Methods

        public async Task DeleteChannelAsync()
        {
            await cf.DeleteChannelAsync(ID);
        }

        public async Task DeleteChannelTagAsync(string tag)
        {
            await cf.DeleteChannelTagAsync(ID, tag);
        }

        public void FillChannelCookieDb()
        {
            cf.FillChannelCookieDb(this);
        }

        public async Task FillChannelCookieNetAsync()
        {
            await cf.FillChannelCookieNetAsync(this);
        }

        public async Task FillChannelDescriptionAsync()
        {
            await cf.FillChannelDescriptionAsync(this);
        }

        public async Task FillChannelItemsDbAsync(string dir, int count, int offset)
        {
            await cf.FillChannelItemsFromDbAsync(this, dir, count, offset);
        }

        public async Task<ICred> GetChannelCredentialsAsync()
        {
            return await cf.GetChannelCredentialsAsync(this);
        }

        public async Task<IEnumerable<IVideoItem>> GetChannelItemsDbAsync(int count, int offset)
        {
            // return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelItemsDbAsync(ID);
            return await cf.GetChannelItemsDbAsync(ID, count, offset);
        }

        public async Task<IEnumerable<string>> GetChannelItemsIdsListDbAsync()
        {
            return await cf.GetChannelItemsIdsListDbAsync(ID);
        }

        public async Task<IEnumerable<string>> GetChannelItemsIdsListNetAsync(int maxresult)
        {
            return await cf.GetChannelItemsIdsListNetAsync(ID, maxresult);
        }

        public async Task<IEnumerable<IVideoItem>> GetChannelItemsNetAsync(int maxresult)
        {
            return await cf.GetChannelItemsNetAsync(this, maxresult);
        }

        public async Task<int> GetChannelPlaylistCountDbAsync()
        {
            return await cf.GetChannelPlaylistCountDbAsync(ID);
        }

        public async Task<IEnumerable<IPlaylist>> GetChannelPlaylistsDbAsync()
        {
            return await cf.GetChannelPlaylistsAsync(ID);
        }

        public async Task<IEnumerable<IPlaylist>> GetChannelPlaylistsNetAsync()
        {
            return await cf.GetChannelPlaylistsNetAsync(ID);
        }

        public async Task<IEnumerable<ITag>> GetChannelTagsAsync()
        {
            return await cf.GetChannelTagsAsync(ID);
        }

        public async Task<IEnumerable<IChannel>> GetRelatedChannelNetAsync(string id, SiteType site)
        {
            return await cf.GetRelatedChannelNetAsync(id, site);
        }

        public async Task InsertChannelAsync()
        {
            await cf.InsertChannelAsync(this);
        }

        public async Task InsertChannelItemsAsync()
        {
            await cf.InsertChannelItemsAsync(this);
        }

        public async Task InsertChannelTagAsync(string tag)
        {
            await cf.InsertChannelTagAsync(ID, tag);
        }

        public async Task RenameChannelAsync(string newName)
        {
            await cf.RenameChannelAsync(ID, newName);
        }

        public void StoreCookies()
        {
            cf.StoreCookies(SiteAdress, ChannelCookies);
        }

        public async Task SyncChannelAsync(bool isSyncPls)
        {
            await cf.SyncChannelAsync(this, isSyncPls);
        }

        public async Task SyncChannelPlaylistsAsync()
        {
            await cf.SyncChannelPlaylistsAsync(this);
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
                ChannelItemsCollectionView.Filter = FilterVideo;
                OnPropertyChanged();
            }
        }

        public string ID { get; set; }

        public bool IsDownloading
        {
            get
            {
                return isDownloading;
            }
            set
            {
                isDownloading = value;
                OnPropertyChanged();
            }
        }

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

        public IVideoItem SelectedItem { get; set; }

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

        public void AddNewItem(IVideoItem item, bool isNew)
        {
            item.IsNewItem = isNew;
            item.State = ItemState.LocalNo;
            item.IsHasLocalFile = false;
            item.Site = Site;

            if (isNew)
            {
                ChannelItems.Insert(0, item);
                CountNew += 1;
            }
            else
            {
                ChannelItems.Add(item);
            }
        }

        public bool FilterVideo(object item)
        {
            var value = (IVideoItem)item;
            if (value == null || value.Title == null)
            {
                return false;
            }

            return value.Title.ToLower().Contains(FilterVideoKey.ToLower());
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
