using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.Extensions;
using Models.Factories;

namespace Models.BO
{
    public class Channel :IChannel, INotifyPropertyChanged
    {
        private readonly ChannelFactory _cf;

        private int _countNew;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public string ID { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Site { get; set; }
        public ObservableCollection<IVideoItem> ChannelItems { get; set; }
        public ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        public List<ITag> Tags { get; set; }
        public int CountNew
        {
            get { return _countNew; }
            set
            {
                _countNew = value; 
                OnPropertyChanged();
            }
        }

        public Channel(IChannelFactory cf)
        {
            _cf = cf as ChannelFactory;
            ChannelItems = new ObservableCollection<IVideoItem>();
            ChannelPlaylists = new ObservableCollection<IPlaylist>();
            Tags = new List<ITag>();
        }

        public Channel(IChannelPOCO channel, IChannelFactory cf)
        {
            _cf = cf as ChannelFactory;
            ID = channel.ID;
            Title = channel.Title;
            SubTitle = channel.SubTitle.WordWrap();
            //LastUpdated = channel.LastUpdated;
            Thumbnail = channel.Thumbnail;
            Site = channel.Site;
            ChannelItems = new ObservableCollection<IVideoItem>();
            ChannelPlaylists = new ObservableCollection<IPlaylist>();
            Tags = new List<ITag>();
        }

        public async Task<List<IVideoItem>> GetChannelItemsDbAsync()
        {
            return await _cf.GetChannelItemsDbAsync(ID);
            //return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelItemsDbAsync(ID);
        }

        public async Task SyncChannelAsync(string dir, bool isSyncPls)
        {
            await _cf.SyncChannelAsync(this, dir, isSyncPls);
        }

        public async Task<int> GetChannelItemsCountDbAsync()
        {
            return await _cf.GetChannelItemsCountDbAsync(ID);
            //return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelItemsCountDbAsync(ID);
        }

        public async Task<int> GetChannelItemsCountNetAsync()
        {
            return await _cf.GetChannelItemsCountNetAsync(ID);
            //return await ((ChannelFactory)ServiceLocator.ChannelFactory).GetChannelItemsCountNetAsync(ID);
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
            //await ((ChannelFactory) ServiceLocator.ChannelFactory).InsertChannelAsync(this);
        }

        public async Task DeleteChannelAsync()
        {
            await _cf.DeleteChannelAsync(ID);
            //await ((ChannelFactory) ServiceLocator.ChannelFactory).DeleteChannelAsync(ID);
        }

        public async Task RenameChannelAsync(string newName)
        {
            await _cf.RenameChannelAsync(ID, newName);
            //await ((ChannelFactory) ServiceLocator.ChannelFactory).RenameChannelAsync(ID, newName);
        }

        public async Task InsertChannelItemsAsync()
        {
            await _cf.InsertChannelItemsAsync(this);
            //await ((ChannelFactory) ServiceLocator.ChannelFactory).InsertChannelItemsAsync(this);
        }

        public async Task<List<IVideoItem>> GetChannelItemsNetAsync(int maxresult) //0 - все видео
        {
            return await _cf.GetChannelItemsNetAsync(ID, maxresult);
            //return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelItemsNetAsync(ID, maxresult);
        }

        public async Task<List<IVideoItem>> GetPopularItemsNetAsync(string regionID, int maxresult)
        {
            return await _cf.GetPopularItemsNetAsync(regionID, maxresult);
            //return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetPopularItemsNetAsync(regionID, maxresult);
        }

        public async Task<List<IVideoItem>> SearchItemsNetAsync(string key, int maxresult)
        {
            return await _cf.SearchItemsNetAsync(key, maxresult);
            //return await ((ChannelFactory) ServiceLocator.ChannelFactory).SearchItemsNetAsync(key, maxresult);
        }

        public async Task<List<IPlaylist>> GetChannelPlaylistsNetAsync()
        {
            return await _cf.GetChannelPlaylistsNetAsync(ID);
            //return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelPlaylistsNetAsync(ID);
        }

        public async Task<List<IPlaylist>> GetChannelPlaylistsAsync()
        {
            return await _cf.GetChannelPlaylistsAsync(ID);
            //return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelPlaylistsAsync(ID);
        }

        public async Task<List<ITag>> GetChannelTagsAsync()
        {
            return await _cf.GetChannelTagsAsync(ID);
            //return await ((ChannelFactory) ServiceLocator.ChannelFactory).GetChannelTagsAsync(ID);
        }

        public async Task InsertChannelTagAsync(string tag)
        {
            await _cf.InsertChannelTagAsync(ID, tag);
            //await ((ChannelFactory) ServiceLocator.ChannelFactory).InsertChannelTagAsync(ID, tag);
        }

        public async Task DeleteChannelTagAsync(string tag)
        {
            await _cf.DeleteChannelTagAsync(ID, tag);
            //await ((ChannelFactory) ServiceLocator.ChannelFactory).DeleteChannelTagAsync(ID, tag);
        }
    }
}
