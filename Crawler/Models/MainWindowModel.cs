using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Extensions;
using Interfaces.Factories;
using Interfaces.Models;
using Microsoft.WindowsAPICodePack.Taskbar;
using Models.BO;
using Ninject;
using SitesAPI;
using Container = IoC.Container;

namespace Crawler.Models
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public readonly List<IVideoItem> Filterlist = new List<IVideoItem>();
        private string _dirPath;
        private string _filter;
        private string _info;
        private bool _isIdle;
        private string _link;
        private string _mpcPath;
        private string _newChannelLink;
        private double _prValue;
        private string _result;
        private Channel _selectedChannel;
        private string _selectedCountry;
        private Cred _selectedCred;
        private Playlist _selectedPlaylist;
        private VideoItem _selectedVideoItem;
        private string _youHeader;
        private string _youPath;
        private string _youRegex = @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)";

        public MainWindowModel()
        {
            BaseFactory = Container.Kernel.Get<ICommonFactory>();
            Channels = new ObservableCollection<Channel>();
            ServiceChannels = new ObservableCollection<Channel>();
            SupportedCreds = new List<Cred>();
            Countries = new List<string> { "RU", "US", "CA", "FR", "DE", "IT", "JP" };
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

                var lst = await s.GetChannelsIdsListDbAsync();

                // получим сначала ID каналов и начнем по одному заполнять список
                var cf = BaseFactory.CreateChannelFactory();

                var lstc = new List<IChannel>();
                foreach (var id in lst)
                {
                    var ch = await cf.GetChannelDbAsync(id);
                    lstc.Add(ch);
                }

                var lsss = lstc.OrderBy(x => x.Title);
                foreach (var channel in lsss)
                {
                    Channels.Add(channel as Channel);
                }

                #region по ID

                // var lst = await s.GetChannelsIdsListDbAsync(); //получим сначала ID каналов и начнем по одному заполнять список

                // var cf = BaseFactory.CreateChannelFactory();

                // foreach (string id in lst)
                // {
                // var ch = await cf.GetChannelDbAsync(id);

                // Channels.Add(ch as Channel);
                // }

                #endregion
                
                #region за раз

                // var lst = await s.GetChannelsListAsync(); //все каналы за раз
                // foreach (var ch in lst.Cast<Channel>())
                // {
                // Channels.Add(ch);
                // }
                #endregion

                if (Channels.Any())
                {
                    SelectedChannel = Channels.First();
                }

                var creds = await s.GetCredListAsync();
                foreach (var cred in creds)
                {
                    var cr = (Cred)cred;
                    SupportedCreds.Add(cr);
                }

                if (SupportedCreds.Any())
                {
                    SelectedCred = SupportedCreds.First();
                }

                CreateServicesChannels();

                // SelectedChannel = ServiceChannels.First();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public async Task SaveNewItem()
        {
            if (IsEditMode)
            {
                if (string.IsNullOrEmpty(NewChannelTitle))
                {
                    MessageBox.Show("Fill channel title");
                    return;
                }
                await EditChannel();
            }
            else
            {
                if (string.IsNullOrEmpty(NewChannelLink))
                {
                    MessageBox.Show("Fill channel link");
                    return;
                }
                await AddNewChannel(NewChannelLink);
            }
        }

        public async Task SyncData()
        {
            try
            {
                PrValue = 0;
                IsIdle = false;
                Result = "Working..";
                var i = 0;
                var prog = TaskbarManager.Instance;
                prog.SetProgressState(TaskbarProgressBarState.Normal);

                foreach (var channel in Channels)
                {
                    // SelectedChannel = channel;
                    i += 1;
                    PrValue = Math.Round((double)(100 * i) / Channels.Count);
                    prog.SetProgressValue((int)PrValue, 100);
                    Info = "Syncing: " + channel.Title;
                    await channel.SyncChannelAsync(DirPath, false);
                }

                prog.SetProgressState(TaskbarProgressBarState.NoProgress);

                PrValue = 0;
                IsIdle = true;
                Result = "Finished!";
                Info = "Total: " + i;
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
                {
                    await savedir.UpdateSettingAsync(DirPath);
                }

                var mpcdir = await sf.GetSettingDbAsync("pathToMpc");
                if (mpcdir.Value != MpcPath)
                {
                    await mpcdir.UpdateSettingAsync(MpcPath);
                }

                var youpath = await sf.GetSettingDbAsync("pathToYoudl");
                if (youpath.Value != YouPath)
                {
                    await youpath.UpdateSettingAsync(YouPath);
                }

                foreach (var cred in SupportedCreds)
                {
                    await cred.UpdateLoginAsync(cred.Login);
                    await cred.UpdatePasswordAsync(cred.Pass);
                }

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

                if (string.IsNullOrEmpty(DirPath))
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    DirPath = path;
                    await savedir.UpdateSettingAsync(path);
                }

                var mpcdir = await sf.GetSettingDbAsync("pathToMpc");
                MpcPath = mpcdir.Value;

                var youpath = await sf.GetSettingDbAsync("pathToYoudl");
                YouPath = youpath.Value;

                if (string.IsNullOrEmpty(YouPath))
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
                Info = string.Format("Time: {0} sec", watch.Elapsed.Seconds);
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

        public async Task AddNewChannelAsync(string channelid, string channeltitle)
        {
            var cf = BaseFactory.CreateChannelFactory();
            var vf = BaseFactory.CreateVideoItemFactory();
            var you = BaseFactory.CreateYouTubeSite();

            var channel = await cf.GetChannelNetAsync(channelid) as Channel;
            if (channel == null)
            {
                throw new Exception("GetChannelNetAsync return null");
            }

            if (!string.IsNullOrEmpty(channeltitle))
            {
                channel.Title = channeltitle;
            }

            await channel.DeleteChannelAsync();
            Channels.Add(channel);
            SelectedChannel = channel;
            var lst = await channel.GetChannelItemsNetAsync(0);
            foreach (var item in lst)
            {
                channel.AddNewItem(item, false);
            }

            await channel.InsertChannelItemsAsync();
            var pls = await channel.GetChannelPlaylistsNetAsync();
            foreach (var pl in pls)
            {
                channel.ChannelPlaylists.Add(pl);
                await pl.InsertPlaylistAsync();
                var plv = await pl.GetPlaylistItemsIdsListNetAsync(); // получим список id плейлиста
                var needcheck = new List<string>();
                foreach (var id in plv)
                {
                    if (channel.ChannelItems.Select(x => x.ID).Contains(id))
                    {
                        await pl.UpdatePlaylistAsync(id); // видео есть на нашем канале - проапдейтим связь
                    }
                    else
                    {
                        needcheck.Add(id); // видео нету - пока добавим в список для дальнейшей проверки
                    }
                }

                var nchanks = CommonExtensions.SplitList(needcheck);

                // разобьем на чанки по 50, чтоб поменьше дергать ютуб
                var trueids = new List<string>();
                foreach (var nchank in nchanks)
                {
                    var lvlite = await you.GetVideosListByIdsLiteAsync(nchank);

                    // получим лайтовые объекты, только id и parentid
                    foreach (var poco in lvlite)
                    {
                        // не наши - пофиг, не нужны
                        if (poco.ParentID != channel.ID)
                        {
                            continue;
                        }
                        trueids.Add(poco.ID); // а вот эти с нашего канала - собираем
                    }
                }

                var truchanks = CommonExtensions.SplitList(trueids); // опять же разобьем на чанки

                foreach (var truchank in truchanks)
                {
                    var lvfull = await you.GetVideosListByIdsAsync(truchank); // ну и начнем получать уже полные объекты

                    foreach (var poco in lvfull)
                    {
                        var v = new VideoItem(poco, vf);
                        channel.AddNewItem(v, false);
                        await v.InsertItemAsync();
                    }
                }
            }

            Result = "Finished";
        }

        public async Task DownloadLink()
        {
            if (string.IsNullOrEmpty(Link))
            {
                return;
            }

            if (!CommonExtensions.IsValidUrl(Link))
            {
                MessageBox.Show("Can't parse URL");
                return;
            }

            if (string.IsNullOrEmpty(_youPath))
            {
                MessageBox.Show("Please, select youtube-dl");
                return;
            }

            if (string.IsNullOrEmpty(DirPath))
            {
                MessageBox.Show("Please, set download directory");
                return;
            }

            var vf = BaseFactory.CreateVideoItemFactory();
            var regex = new Regex(_youRegex);
            var match = regex.Match(Link);
            if (match.Success)
            {
                var id = match.Groups[1].Value;
                var vi = await vf.GetVideoItemNetAsync(id);
                vi.ParentID = null;

                SelectedVideoItem = vi as VideoItem;
                SelectedChannel = ServiceChannels.First();

                ServiceChannels.First().AddNewItem(vi, true);
                await vi.DownloadItem(_youPath, DirPath, IsHd);
                vi.IsNewItem = true;
            }
            else
            {
                var param = string.Format("-o {0}\\%(title)s.%(ext)s {1} --no-check-certificate -i --console-title", DirPath, Link);
                await Task.Run(() =>
                {
                    var process = Process.Start(_youPath, param);
                    if (process != null)
                    {
                        process.Close();
                    }
                });
            }
        }

        public async Task AddNewChannel(string inputChannelId)
        {
            Result = "Working..";
            Info = string.Empty;
            const string youUser = "user";
            const string youChannel = "channel";
            var parsedChannelId = string.Empty;
            var cf = BaseFactory.CreateChannelFactory();

            var sp = inputChannelId.Split('/');
            if (sp.Length > 1)
            {
                if (sp.Contains(youUser))
                {
                    var indexuser = Array.IndexOf(sp, youUser);
                    if (indexuser < 0)
                    {
                        throw new Exception("Can't parse url");
                    }

                    var user = sp[indexuser + 1];
                    parsedChannelId = await cf.GetChannelIdByUserNameNetAsync(user);
                }
                else if (sp.Contains(youChannel))
                {
                    var indexchannel = Array.IndexOf(sp, youChannel);
                    if (indexchannel < 0)
                    {
                        throw new Exception("Can't parse url");
                    }

                    parsedChannelId = sp[indexchannel + 1];
                }
                else
                {
                    var regex = new Regex(_youRegex);
                    var match = regex.Match(inputChannelId);
                    if (match.Success)
                    {
                        var id = match.Groups[1].Value;
                        var vi = await BaseFactory.CreateYouTubeSite().GetVideoItemLiteNetAsync(id);
                        parsedChannelId = vi.ParentID;
                    }
                }
            }
            else
            {
                try
                {
                    parsedChannelId = await cf.GetChannelIdByUserNameNetAsync(inputChannelId);
                }
                catch
                {
                    parsedChannelId = inputChannelId;
                }
            }

            if (Channels.Select(x => x.ID).Contains(parsedChannelId))
            {
                MessageBox.Show("Has already");
                return;
            }

            if (!string.IsNullOrEmpty(parsedChannelId))
            {
                await AddNewChannelAsync(parsedChannelId, NewChannelTitle);
            }
        }

        private async Task EditChannel()
        {
            SelectedChannel.Title = NewChannelTitle;
            await SelectedChannel.RenameChannelAsync(NewChannelTitle);
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

        private void FilterVideos()
        {
            if (SelectedChannel == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(Filter))
            {
                if (!Filterlist.Any())
                {
                    return;
                }

                foreach (var item in SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = false;
                }

                foreach (var item in Filterlist)
                {
                    var vid = SelectedChannel.ChannelItems.FirstOrDefault(x => x.ID == item.ID);
                    if (vid != null)
                    {
                        vid.IsShowRow = true;
                    }
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

                foreach (var item in SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = false;
                }

                foreach (var item in Filterlist)
                {
                    if (item.Title.ToLower().Contains(Filter.ToLower()))
                    {
                        var vid = SelectedChannel.ChannelItems.FirstOrDefault(x => x.ID == item.ID);
                        if (vid != null)
                        {
                            vid.IsShowRow = true;
                        }
                    }
                }
            }
        }

        #region Fields

        public ICommonFactory BaseFactory { get; set; }

        public ObservableCollection<Channel> Channels { get; set; }

        public ObservableCollection<Channel> ServiceChannels { get; set; }

        public List<string> Countries { get; set; }

        public Channel SelectedChannel
        {
            get
            {
                return _selectedChannel;
            }
            set
            {
                _selectedChannel = value;
                OnPropertyChanged();
            }
        }

        public VideoItem SelectedVideoItem
        {
            get
            {
                return _selectedVideoItem;
            }
            set
            {
                _selectedVideoItem = value;
                OnPropertyChanged();
            }
        }

        public Playlist SelectedPlaylist
        {
            get
            {
                return _selectedPlaylist;
            }
            set
            {
                _selectedPlaylist = value;
                OnPropertyChanged();
            }
        }

        public List<Cred> SupportedCreds { get; set; }

        public Cred SelectedCred
        {
            get
            {
                return _selectedCred;
            }
            set
            {
                _selectedCred = value;
                OnPropertyChanged();
            }
        }

        public string NewChannelLink
        {
            get
            {
                return _newChannelLink;
            }
            set
            {
                _newChannelLink = value;
                OnPropertyChanged();
            }
        }

        public string NewChannelTitle { get; set; }

        public string Info
        {
            get
            {
                return _info;
            }
            set
            {
                _info = value;
                OnPropertyChanged();
            }
        }

        public string Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }

        public string DirPath
        {
            get
            {
                return _dirPath;
            }
            set
            {
                _dirPath = value;
                OnPropertyChanged();
            }
        }

        public string MpcPath
        {
            get
            {
                return _mpcPath;
            }
            set
            {
                _mpcPath = value;
                OnPropertyChanged();
            }
        }

        public string YouPath
        {
            get
            {
                return _youPath;
            }
            set
            {
                _youPath = value;
                OnPropertyChanged();
            }
        }

        public double PrValue
        {
            get
            {
                return _prValue;
            }
            set
            {
                _prValue = value;
                OnPropertyChanged();
            }
        }

        public string YouHeader
        {
            get
            {
                return _youHeader;
            }
            set
            {
                _youHeader = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCountry
        {
            get
            {
                return _selectedCountry;
            }
            set
            {
                _selectedCountry = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditMode { get; set; }

        public bool IsIdle
        {
            get
            {
                return _isIdle;
            }
            set
            {
                _isIdle = value;
                OnPropertyChanged();
            }
        }

        public string Version { get; set; }

        public string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
                FilterVideos();
            }
        }

        public string Link
        {
            get
            {
                return _link;
            }
            set
            {
                _link = value;
                OnPropertyChanged();
            }
        }

        public bool IsHd { get; set; }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
