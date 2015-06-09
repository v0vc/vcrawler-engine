using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Extensions;
using Interfaces.Factories;
using Interfaces.Models;
using Models.BO;
using Ninject;
using SitesAPI;
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
        private string _newChannelLink;
        private VideoItem _selectedVideoItem;
        private string _result;
        private Playlist _selectedPlaylist;
        private string _dirPath;
        private string _mpcPath;
        private string _youPath;
        private double _prValue;
        private string _youHeader;
        private string _selectedCountry;
        private bool _isIdle;
        private string _filter;
        public readonly List<IVideoItem> Filterlist = new List<IVideoItem>();

        #region Fields

        public ICommonFactory BaseFactory { get; set; }

        public ObservableCollection<Channel> Channels { get; set; }

        public ObservableCollection<Channel> ServiceChannels { get; set; }

        public List<string> Countries { get; set; }

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

        public string NewChannelLink
        {
            get { return _newChannelLink; }
            set
            {
                _newChannelLink = value;
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

        public string SelectedCountry
        {
            get { return _selectedCountry; }
            set
            {
                _selectedCountry = value; 
                OnPropertyChanged();
            }
        }

        public bool IsEditMode { get; set; }

        public bool IsIdle
        {
            get { return _isIdle; }
            set
            {
                _isIdle = value; 
                OnPropertyChanged();
            }
        }

        public string Version { get; set; }

        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value; 
                FilterVideos();
            }
        }

        #endregion

        public MainWindowModel()
        {
            BaseFactory = Container.Kernel.Get<ICommonFactory>();
            Channels = new ObservableCollection<Channel>();
            ServiceChannels = new ObservableCollection<Channel>();
            SupportedCreds = new List<Cred>();
            Countries = new List<string> {"RU", "US", "CA", "FR", "DE", "IT", "JP"};
            SelectedCountry = Countries.First();
            IsIdle = true;
            Version = CommonExtensions.GetFileVersion(Assembly.GetExecutingAssembly());
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
                foreach (var ch in lst.Cast<Channel>())
                {
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

                CreateServicesChannels();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //Info = Regex.Replace(ex.Message, @"\t|\n|\r", "-");
            }
        }

        private void CreateServicesChannels()
        {
            var cf = BaseFactory.CreateChannelFactory();
            var chpop = cf.CreateChannel();
            chpop.Title = "#Popular";
            chpop.Site = "youtube.com";
            var img = Assembly.GetExecutingAssembly().GetManifestResourceStream("Crawler.Images.pop.png");
            chpop.Thumbnail = SiteHelper.ReadFully(img);
            chpop.ID = "pop";
            ServiceChannels.Add(chpop as Channel);
        }

        public async Task SaveNewItem()
        {
            if (IsEditMode)
            {
                if (String.IsNullOrEmpty(NewChannelTitle))
                {
                    MessageBox.Show("Fill channel title");
                    return;
                }
                await EditChannel();
            }
            else
            {
                if (String.IsNullOrEmpty(NewChannelLink))
                {
                    MessageBox.Show("Fill channel link");
                    return;
                }
                await AddNewChannel();
            }
        }

        public async Task SyncData()
        {
            try
            {
                var watch = Stopwatch.StartNew();

                PrValue = 0;

                IsIdle = false;

                Result = "Working..";

                var i = 0;

                foreach (Channel channel in Channels)
                {
                    i += 1;
                    PrValue = Math.Round((double) (100*i)/Channels.Count);
                    Info = "Syncing: " + channel.Title;
                    await channel.SyncChannelAsync(DirPath, false);
                }

                PrValue = 0;

                IsIdle = true;

                Result = "Finished!";

                watch.Stop();

                Info = String.Format("Time: {0} sec", watch.Elapsed.Seconds);
            }
            catch (Exception ex)
            {
                IsIdle = true;
                Result = "Error";
                Info = ex.Message;
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

                if (String.IsNullOrEmpty(DirPath))
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    DirPath = path;
                    await savedir.UpdateSettingAsync(path);
                }

                var mpcdir = await sf.GetSettingDbAsync("pathToMpc");
                MpcPath = mpcdir.Value;

                var youpath = await sf.GetSettingDbAsync("pathToYoudl");
                YouPath = youpath.Value;

                if (String.IsNullOrEmpty(YouPath))
                {
                    var path = AppDomain.CurrentDomain.BaseDirectory;
                    var res = Path.Combine(path, "youtube-dl.exe");
                    var fn = new FileInfo(res);
                    if (fn.Exists)
                    {
                        YouPath = fn.FullName;
                        await youpath.UpdateSettingAsync(fn.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                Info = ex.Message;
            }
        }

        public async Task SyncChannel(IChannel channel)
        {
            Info = "Syncing: " + channel.Title;
            try
            {
                IsIdle = false;
                var watch = Stopwatch.StartNew();
                await channel.SyncChannelAsync(DirPath, true);
                watch.Stop();
                Info = String.Format("Time: {0} sec", watch.Elapsed.Seconds);
                Result = "Finished!";
                IsIdle = true;
            }
            catch (Exception ex)
            {
                IsIdle = true;
                Result = "Error";
                Info = ex.Message;
                MessageBox.Show(ex.Message);
            }
        }

        private async Task AddNewChannel()
        {
            Result = "Working..";

            var cf = BaseFactory.CreateChannelFactory();
            var vf = BaseFactory.CreateVideoItemFactory();

            var sp = NewChannelLink.Split('/');
            if (sp.Length > 1)
            {
                if (sp.Contains("user"))
                {
                    NewChannelLink = await cf.GetChannelIdByUserNameNetAsync(sp.Last());
                }

                if (sp.Contains("channel"))
                    NewChannelLink = sp.Last();
            }
            else
            {
                try
                {
                    NewChannelLink = await cf.GetChannelIdByUserNameNetAsync(NewChannelLink);
                }
                catch
                {
                    NewChannelLink = NewChannelLink; // :)
                }
            }


            if (Channels.Select(x => x.ID).Contains(NewChannelLink))
            {
                MessageBox.Show("Has already");
                return;
            }

            try
            {
                var channel = await cf.GetChannelNetAsync(NewChannelLink) as Channel;
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
                        else
                        {
                            var vid = await vf.GetVideoItemNetAsync(id);
                            channel.AddNewItem(vid);
                            await vid.InsertItemAsync();
                        }
                    }
                }

                Result = "Finished";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task EditChannel()
        {
            SelectedChannel.Title = NewChannelTitle;
            await SelectedChannel.RenameChannelAsync(NewChannelTitle);
        }

        private void FilterVideos()
        {
            if (SelectedChannel == null)
                return;

            if (string.IsNullOrEmpty(Filter))
            {
                if (!Filterlist.Any())
                    return;

                foreach (IVideoItem item in SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = false;
                }

                foreach (var item in Filterlist)
                {
                    var vid = SelectedChannel.ChannelItems.FirstOrDefault(x => x.ID == item.ID);
                    if (vid != null)
                        vid.IsShowRow = true;
                }

                Filterlist.Clear();
            }
            else
            {
                if (!Filterlist.Any())
                {
                    foreach (var item in SelectedChannel.ChannelItems.Where(item => item.IsShowRow))
                    {
                        Filterlist.Add(item);
                    }
                }
                foreach (IVideoItem item in SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = false;
                }
                foreach (var item in Filterlist)
                {
                    if (item.Title.ToLower().Contains(Filter.ToLower()))
                    {
                        var vid = SelectedChannel.ChannelItems.FirstOrDefault(x => x.ID == item.ID);
                        if (vid != null)
                            vid.IsShowRow = true;
                    }
                }
            }
        }

    }
}
