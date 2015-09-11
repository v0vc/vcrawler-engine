// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO
{
    public sealed class Channel : IChannel, INotifyPropertyChanged
    {
        #region Static and Readonly Fields

        private readonly ChannelFactory _cf;

        #endregion

        #region Fields

        private int _countNew;
        private bool _isDownloading;
        private bool _isInWork;
        private bool _isShowRow;
        private int _playlistCount;
        private string _subTitle;
        private string _title;

        #endregion

        #region Constructors

        public Channel(ChannelFactory cf)
        {
            _cf = cf;
        }

        private Channel()
        {
        }

        #endregion

        #region Methods

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
        public ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        public ObservableCollection<ITag> ChannelTags { get; set; }

        public int CountNew
        {
            get
            {
                return _countNew;
            }
            set
            {
                _countNew = value;
                OnPropertyChanged();
            }
        }

        public string ID { get; set; }

        public bool IsDownloading
        {
            get
            {
                return _isDownloading;
            }
            set
            {
                _isDownloading = value;
                OnPropertyChanged();
            }
        }

        public bool IsInWork
        {
            get
            {
                return _isInWork;
            }
            set
            {
                _isInWork = value;
                OnPropertyChanged();
            }
        }

        public bool IsShowRow
        {
            get
            {
                return _isShowRow;
            }
            set
            {
                _isShowRow = value;
                OnPropertyChanged();
            }
        }

        public int PlaylistCount
        {
            get
            {
                return _playlistCount;
            }
            set
            {
                _playlistCount = value;
                OnPropertyChanged();
            }
        }

        public string Site { get; set; }

        public string SubTitle
        {
            get
            {
                return _subTitle;
            }
            set
            {
                _subTitle = value;
                OnPropertyChanged();
            }
        }

        public byte[] Thumbnail { get; set; }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public void AddNewItem(IVideoItem item, bool isNew)
        {
            item.IsNewItem = isNew;
            item.IsShowRow = true;
            item.ItemState = "LocalNo";
            item.IsHasLocalFile = false;

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

        public async Task DeleteChannelAsync()
        {
            await _cf.DeleteChannelAsync(ID);
        }

        public async Task DeleteChannelTagAsync(string tag)
        {
            await _cf.DeleteChannelTagAsync(ID, tag);
        }

        public void FillChannelCookieDb()
        {
            _cf.FillChannelCookieDb(this);
        }

        public async Task FillChannelCookieNetAsync()
        {
            await _cf.FillChannelCookieNetAsync(this);
        }

        public async Task FillChannelDescriptionAsync()
        {
            await _cf.FillChannelDescriptionAsync(this);
        }

        public async Task FillChannelItemsDbAsync(string dir)
        {
            await _cf.FillChannelItemsFromDbAsync(this, dir);
        }

        public async Task<int> GetChannelItemsCountDbAsync()
        {
            return await _cf.GetChannelItemsCountDbAsync(ID);
        }

        public async Task<int> GetChannelItemsCountNetAsync()
        {
            return await _cf.GetChannelItemsCountNetAsync(ID);
        }

        public async Task<IEnumerable<IVideoItem>> GetChannelItemsDbAsync()
        {
            // return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelItemsDbAsync(ID);
            return await _cf.GetChannelItemsDbAsync(ID);
        }

        public async Task<IEnumerable<string>> GetChannelItemsIdsListDbAsync()
        {
            return await _cf.GetChannelItemsIdsListDbAsync(ID);
        }

        public async Task<IEnumerable<string>> GetChannelItemsIdsListNetAsync(int maxresult)
        {
            return await _cf.GetChannelItemsIdsListNetAsync(ID, maxresult);
        }

        public async Task<IEnumerable<IVideoItem>> GetChannelItemsNetAsync(int maxresult)
        {
            return await _cf.GetChannelItemsNetAsync(this, maxresult);
        }

        public async Task<int> GetChannelPlaylistCountDbAsync()
        {
            return await _cf.GetChannelPlaylistCountDbAsync(ID);
        }

        public async Task<IEnumerable<IPlaylist>> GetChannelPlaylistsDbAsync()
        {
            return await _cf.GetChannelPlaylistsAsync(ID);
        }

        public async Task<IEnumerable<IPlaylist>> GetChannelPlaylistsNetAsync()
        {
            return await _cf.GetChannelPlaylistsNetAsync(ID);
        }

        public async Task<IEnumerable<ITag>> GetChannelTagsAsync()
        {
            return await _cf.GetChannelTagsAsync(ID);
        }

        public async Task<IEnumerable<IVideoItem>> GetPopularItemsNetAsync(string regionID, int maxresult)
        {
            return await _cf.GetPopularItemsNetAsync(regionID, maxresult);
        }

        public async Task InsertChannelAsync()
        {
            await _cf.InsertChannelAsync(this);
        }

        public async Task InsertChannelItemsAsync()
        {
            await _cf.InsertChannelItemsAsync(this);
        }

        public async Task InsertChannelTagAsync(string tag)
        {
            await _cf.InsertChannelTagAsync(ID, tag);
        }

        public async Task RenameChannelAsync(string newName)
        {
            await _cf.RenameChannelAsync(ID, newName);
        }

        public async Task<IEnumerable<IVideoItem>> SearchItemsNetAsync(string key, string region, int maxresult)
        {
            return await _cf.SearchItemsNetAsync(key, region, maxresult);
        }

        public void StoreCookies()
        {
            _cf.StoreCookies(Site, ChannelCookies);
        }

        public async Task SyncChannelAsync(bool isSyncPls)
        {
            await _cf.SyncChannelAsync(this, isSyncPls);
        }

        public async Task SyncChannelPlaylistsAsync()
        {
            await _cf.SyncChannelPlaylistsAsync(this);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
