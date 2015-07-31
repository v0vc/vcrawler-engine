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
        private readonly ChannelFactory _cf;
        private int _countNew;
        private bool _isDownloading;
        private string _title;
        private bool _isInWork;

        private Channel()
        {
        }

        public Channel(ChannelFactory cf)
        {
            _cf = cf;
        }

        public string ID { get; set; }

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

        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Site { get; set; }
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

        public CookieCollection ChannelCookies { get; set; }

        public async Task<List<IVideoItem>> GetChannelItemsDbAsync()
        {
            // return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelItemsDbAsync(ID);
            return await _cf.GetChannelItemsDbAsync(ID);
        }

        public async Task SyncChannelAsync(string dir, bool isSyncPls)
        {
            await _cf.SyncChannelAsync(this, dir, isSyncPls);
        }

        public async Task SyncChannelPlaylistsAsync()
        {
            await _cf.SyncChannelPlaylistsAsync(this);
        }

        public async Task<int> GetChannelItemsCountDbAsync()
        {
            return await _cf.GetChannelItemsCountDbAsync(ID);
        }

        public async Task<int> GetChannelItemsCountNetAsync()
        {
            return await _cf.GetChannelItemsCountNetAsync(ID);
        }

        public async Task<List<string>> GetChannelItemsIdsListNetAsync(int maxresult)
        {
            return await _cf.GetChannelItemsIdsListNetAsync(ID, maxresult);
        }

        public async Task<List<string>> GetChannelItemsIdsListDbAsync()
        {
            return await _cf.GetChannelItemsIdsListDbAsync(ID);
        }

        public async Task FillChannelItemsDbAsync(string dir)
        {
            await _cf.FillChannelItemsFromDbAsync(this, dir);
        }

        public async Task InsertChannelAsync()
        {
            await _cf.InsertChannelAsync(this);
        }

        public async Task DeleteChannelAsync()
        {
            await _cf.DeleteChannelAsync(ID);
        }

        public async Task RenameChannelAsync(string newName)
        {
            await _cf.RenameChannelAsync(ID, newName);
        }

        public async Task InsertChannelItemsAsync()
        {
            await _cf.InsertChannelItemsAsync(this);
        }

        public async Task<List<IVideoItem>> GetChannelItemsNetAsync(int maxresult)
        {
            return await _cf.GetChannelItemsNetAsync(this, maxresult);
        }

        public async Task<List<IVideoItem>> GetPopularItemsNetAsync(string regionID, int maxresult)
        {
            return await _cf.GetPopularItemsNetAsync(regionID, maxresult);
        }

        public async Task<List<IVideoItem>> SearchItemsNetAsync(string key, string region, int maxresult)
        {
            return await _cf.SearchItemsNetAsync(key, region, maxresult);
        }

        public async Task<List<IPlaylist>> GetChannelPlaylistsNetAsync()
        {
            return await _cf.GetChannelPlaylistsNetAsync(ID);
        }

        public async Task<List<IPlaylist>> GetChannelPlaylistsAsync()
        {
            return await _cf.GetChannelPlaylistsAsync(ID);
        }

        public async Task<List<ITag>> GetChannelTagsAsync()
        {
            return await _cf.GetChannelTagsAsync(ID);
        }

        public async Task InsertChannelTagAsync(string tag)
        {
            await _cf.InsertChannelTagAsync(ID, tag);
        }

        public async Task DeleteChannelTagAsync(string tag)
        {
            await _cf.DeleteChannelTagAsync(ID, tag);
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

        public async Task FillChannelCookieNetAsync()
        {
            await _cf.FillChannelCookieNetAsync(this);
        }

        public async Task StoreCookiesAsync()
        {
            await _cf.StoreCookiesAsync(Site, ChannelCookies);
        }

        public async Task FillChannelCookieDbAsync()
        {
            await _cf.FillChannelCookieDbAsync(this);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
