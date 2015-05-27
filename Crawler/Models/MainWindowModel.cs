using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Interfaces.Factories;
using Interfaces.Models;
using Models.BO;
using Ninject;
using Container = IoC.Container;

namespace Crawler.Models
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private string _info;
        private Cred _selectedCred;
        private Channel _selectedChannel;
        private string _newChannelName;
        private VideoItem _selectedVideoItem;
        private string _result;
        private Playlist _selectedPlaylist;
        private string _dirPath;
        private string _mpcPath;
        private string _youPath;
        //private string _ffmegPath;
        private double _prValue;
        private string _youHeader;

        #region Fields

        public ICommonFactory BaseFactory { get; set; }

        public ObservableCollection<Channel> Channels { get; set; }

        public Channel SelectedChannel
        {
            get { return _selectedChannel; }
            set
            {
                _selectedChannel = value;
                OnPropertyChanged();
            }
        }

        public VideoItem SelectedVideoItem
        {
            get { return _selectedVideoItem; }
            set
            {
                _selectedVideoItem = value;
                OnPropertyChanged();
            }
        }

        public Playlist SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set
            {
                _selectedPlaylist = value; 
                OnPropertyChanged();
            }
        }

        public List<Cred> SupportedCreds { get; set; }

        public Cred SelectedCred
        {
            get { return _selectedCred; }
            set
            {
                _selectedCred = value;
                OnPropertyChanged();
            }
        }

        public string NewChannelName
        {
            get { return _newChannelName; }
            set
            {
                _newChannelName = value;
                OnPropertyChanged();
            }
        }

        public string NewChannelTitle { get; set; }

        public string Info
        {
            get { return _info; }
            set
            {
                _info = value;
                OnPropertyChanged();
            }
        }

        public string Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }

        public string DirPath
        {
            get { return _dirPath; }
            set
            {
                _dirPath = value; 
                OnPropertyChanged();
            }
        }

        public string MpcPath
        {
            get { return _mpcPath; }
            set
            {
                _mpcPath = value; 
                OnPropertyChanged();
            }
        }

        public string YouPath
        {
            get { return _youPath; }
            set
            {
                _youPath = value; 
                OnPropertyChanged();
            }
        }

        //public string FfmegPath
        //{
        //    get { return _ffmegPath; }
        //    set
        //    {
        //        _ffmegPath = value; 
        //        OnPropertyChanged();
        //    }
        //}

        public double PrValue
        {
            get { return _prValue; }
            set
            {
                _prValue = value; 
                OnPropertyChanged();
            }
        }

        public string YouHeader
        {
            get { return _youHeader; }
            set
            {
                _youHeader = value; 
                OnPropertyChanged();
            }
        }

        #endregion

        public MainWindowModel()
        {
            BaseFactory = Container.Kernel.Get<ICommonFactory>();
            Channels = new ObservableCollection<Channel>();
            SupportedCreds = new List<Cred>();
        }

        public async Task FillChannels()
        {
            var sf = BaseFactory.CreateSubscribeFactory();

            var s = sf.GetSubscribe();

            try
            {
                await LoadSettings();

                //var lst = await s.GetChannelsIdsListDbAsync(); //получим сначала ID каналов и начнем по одному заполнять список

                //var cf = BaseFactory.CreateChannelFactory();

                //foreach (string id in lst)
                //{
                //    var ch = await cf.GetChannelDbAsync(id);

                //    Channels.Add(ch as Channel);
                //}

                var lst = await s.GetChannelsListAsync(); //все каналы за раз
                foreach (var channel in lst)
                {
                    var ch = (Channel)channel;
                    //ch.CountNew = 1;
                    Channels.Add(ch);
                }

                if (Channels.Any())
                    SelectedChannel = Channels.First();

                var creds = await s.GetCredListAsync();
                foreach (var cred in creds)
                {
                    var cr = (Cred)cred;
                    SupportedCreds.Add(cr);
                }

                if (SupportedCreds.Any())
                    SelectedCred = SupportedCreds.First();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //Info = Regex.Replace(ex.Message, @"\t|\n|\r", "-");
            }
        }

        public async Task SaveNewItem()
        {
            if (String.IsNullOrEmpty(NewChannelName))
            {
                MessageBox.Show("Fill user name");
                return;
            }

            Result = "Working..";

            var cf = BaseFactory.CreateChannelFactory();

            var sp = NewChannelName.Split('/');
            if (sp.Length > 1)
            {
                if (sp.Contains("user"))
                {
                    NewChannelName = await cf.GetChannelIdByUserNameNetAsync(sp.Last());
                }

                if (sp.Contains("channel"))
                    NewChannelName = sp.Last();
            }
            else
            {
                try
                {
                    NewChannelName = await cf.GetChannelIdByUserNameNetAsync(NewChannelName);
                }
                catch
                {
                    NewChannelName = NewChannelName; // :)
                }    
            }
            

            if (Channels.Select(x => x.ID).Contains(NewChannelName))
            {
                MessageBox.Show("Has already");
                return;
            }

            try
            {
                var channel = await cf.GetChannelNetAsync(NewChannelName) as Channel;
                if (channel == null)
                    throw new Exception("GetChannelNetAsync return null");

                if (!String.IsNullOrEmpty(NewChannelTitle))
                    channel.Title = NewChannelTitle;

                Channels.Add(channel);

                SelectedChannel = channel;

                var lst = await channel.GetChannelItemsNetAsync(0);

                foreach (IVideoItem item in lst)
                {
                    item.IsShowRow = true;
                    item.ItemState = "LocalNo";
                    item.IsHasLocalFile = false;
                    channel.ChannelItems.Add(item);
                }

                await channel.InsertChannelItemsAsync();

                var pls = await channel.GetChannelPlaylistsNetAsync();

                foreach (var pl in pls)
                {
                    channel.ChannelPlaylists.Add(pl);

                    await pl.InsertPlaylistAsync();

                    var plv = await pl.GetPlaylistItemsIdsListNetAsync();

                    foreach (string id in plv)
                    {
                        if (channel.ChannelItems.Select(x => x.ID).Contains(id))
                            await pl.UpdatePlaylistAsync(id);
                    }
                }

                Result = "Finished";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public async Task SyncData()
        {
            try
            {
                Result = "Working..";

                foreach (Channel channel in Channels)
                {
                    await channel.SyncChannelAsync(DirPath, false);
                }

                Result = "Finished";
            }
            catch (Exception ex)
            {
                Result = "Error";
                MessageBox.Show(ex.Message);
            }
        }

        public async Task SaveSettings()
        {
            var sf = BaseFactory.CreateSettingFactory();

            try
            {
                var savedir = await sf.GetSettingDbAsync("pathToDownload");
                if (savedir.Value != DirPath)
                    await savedir.UpdateSettingAsync(DirPath);


                var mpcdir = await sf.GetSettingDbAsync("pathToMpc");
                if (mpcdir.Value != MpcPath)
                    await mpcdir.UpdateSettingAsync(MpcPath);

                var youpath = await sf.GetSettingDbAsync("pathToYoudl");
                if (youpath.Value != YouPath)
                    await youpath.UpdateSettingAsync(YouPath);

                //var fpath = await sf.GetSettingDbAsync("pathToFfmpeg");
                //if (fpath.Value != FfmegPath)
                //    await fpath.UpdateSettingAsync(FfmegPath);

                Result = "Saved";
            }
            catch (Exception ex)
            {
                Info = ex.Message;
            }
        }

        public async Task LoadSettings()
        {
            var sf = BaseFactory.CreateSettingFactory();

            try
            {
                var savedir = await sf.GetSettingDbAsync("pathToDownload");
                DirPath = savedir.Value;

                var mpcdir = await sf.GetSettingDbAsync("pathToMpc");
                MpcPath = mpcdir.Value;

                var youpath = await sf.GetSettingDbAsync("pathToYoudl");
                YouPath = youpath.Value;

                //var fpath = await sf.GetSettingDbAsync("pathToFfmpeg");
                //FfmegPath = fpath.Value;

            }
            catch (Exception ex)
            {
                Info = ex.Message;
            }
        }
    }
}
