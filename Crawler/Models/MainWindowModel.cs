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
using Interfaces.API;
using Interfaces.Factories;
using Interfaces.Models;
using Microsoft.WindowsAPICodePack.Taskbar;
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
        private IChannel _selectedChannel;
        private string _selectedCountry;
        private ICred _selectedCred;
        private IPlaylist _selectedPlaylist;
        private IVideoItem _selectedVideoItem;
        private string _youHeader;
        private string _youPath;
        private readonly string _youRegex = @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)";
        private string _searchKey;
        private const string PathToDownload = "pathToDownload";
        private const string PathToMpc = "pathToMpc";
        private const string PathToYoudl = "pathToYoudl";
        private const string YoutubeDl = "youtube-dl.exe";

        private readonly IChannelFactory _cf;
        private readonly IVideoItemFactory _vf;
        private readonly ISettingFactory _sf;
        private readonly IYouTubeSite _yf;
        private readonly ISqLiteDatabase _df;
        private readonly ICredFactory _crf;
        private string _newTag;

        public MainWindowModel()
        {
            Channels = new ObservableCollection<IChannel>();
            ServiceChannels = new ObservableCollection<IChannel>();
            RelatedChannels = new ObservableCollection<IChannel>();
            SupportedCreds = new List<ICred>();
            Tags = new ObservableCollection<ITag>();
            Countries = new List<string> { "RU", "US", "CA", "FR", "DE", "IT", "JP" };
            SelectedCountry = Countries.First();
            IsIdle = true;
            Version = CommonExtensions.GetFileVersion(Assembly.GetExecutingAssembly());

            BaseFactory = Container.Kernel.Get<ICommonFactory>();
            _cf = BaseFactory.CreateChannelFactory();
            _vf = BaseFactory.CreateVideoItemFactory();
            _sf = BaseFactory.CreateSettingFactory();
            _yf = BaseFactory.CreateYouTubeSite();
            _df = BaseFactory.CreateSqLiteDatabase();
            _crf = BaseFactory.CreateCredFactory();
        }

        public void SetStatus(int res)
        {
            if (res == 0)
            {
                Result = "Ready";
            }
            if (res == 1)
            {
                Result = "Working..";
            }
            if (res == 2)
            {
                Result = "Finished!";
            }
            if (res == 3)
            {
                Result = "Error";
            }
        }

        public async Task FillChannels()
        {
            await LoadSettings();

            

            // var channelIds = await GetChannelsIdsListDbAsync();

            // var channelsTemp = new List<IChannel>();
            // foreach (var id in channelIds)
            // {
            // var ch = await _cf.GetChannelDbAsync(id);
            // channelsTemp.Add(ch);
            // }

            // var channelsSorted = channelsTemp.OrderBy(x => x.Title);
            // foreach (var channel in channelsSorted)
            // {
            // Channels.Add(channel);
            // }

            
            #region по ID

            // var lst = await GetChannelsIdsListDbAsync(); //получим сначала ID каналов и начнем по одному заполнять список

            // foreach (string id in lst)
            // {
            // var ch = await _cf.GetChannelDbAsync(id);

            // Channels.Add(ch);
            // }
            #endregion

            #region за раз

            var lst = await GetChannelsListAsync(); // все каналы за раз
            foreach (var ch in lst)
            {
                Channels.Add(ch);
            }

            #endregion

            if (Channels.Any())
            {
                SelectedChannel = Channels.First();
            }

            CreateServicesChannels();
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
            PrValue = 0;
            IsIdle = false;
            SetStatus(1);
            var i = 0;
            var prog = TaskbarManager.Instance;
            prog.SetProgressState(TaskbarProgressBarState.Normal);

            try
            {
                foreach (var channel in Channels)
                {
                    i += 1;
                    PrValue = Math.Round((double)(100 * i) / Channels.Count);
                    prog.SetProgressValue((int)PrValue, 100);
                    Info = "Syncing: " + channel.Title;
                    await channel.SyncChannelAsync(DirPath, false);
                }

                prog.SetProgressState(TaskbarProgressBarState.NoProgress);

                PrValue = 0;
                IsIdle = true;
                SetStatus(2);
                Info = "Total: " + i;
            }
            catch (Exception ex)
            {
                IsIdle = true;
                SetStatus(3);
                Info = ex.Message;
                MessageBox.Show(ex.Message);
            }
        }

        public async Task SaveSettings()
        {
            try
            {
                SetStatus(1);
                var savedir = await _sf.GetSettingDbAsync(PathToDownload);
                if (savedir.Value != DirPath)
                {
                    await savedir.UpdateSettingAsync(DirPath);
                }

                var mpcdir = await _sf.GetSettingDbAsync(PathToMpc);
                if (mpcdir.Value != MpcPath)
                {
                    await mpcdir.UpdateSettingAsync(MpcPath);
                }

                var youpath = await _sf.GetSettingDbAsync(PathToYoudl);
                if (youpath.Value != YouPath)
                {
                    await youpath.UpdateSettingAsync(YouPath);
                }

                foreach (var cred in SupportedCreds)
                {
                    await cred.UpdateLoginAsync(cred.Login);
                    await cred.UpdatePasswordAsync(cred.Pass);
                }

                foreach (ITag tag in Tags)
                {
                    await tag.InsertTagAsync();
                }

                SetStatus(0);
            }
            catch (Exception ex)
            {
                Info = ex.Message;
                SetStatus(3);
            }
        }

        private async Task LoadSettings()
        {
            try
            {
                var savedir = await _sf.GetSettingDbAsync(PathToDownload);
                DirPath = savedir.Value;

                if (string.IsNullOrEmpty(DirPath))
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    DirPath = path;
                    await savedir.UpdateSettingAsync(path);
                }

                var mpcdir = await _sf.GetSettingDbAsync(PathToMpc);
                MpcPath = mpcdir.Value;

                var youpath = await _sf.GetSettingDbAsync(PathToYoudl);
                YouPath = youpath.Value;

                if (string.IsNullOrEmpty(YouPath))
                {
                    var path = AppDomain.CurrentDomain.BaseDirectory;
                    var res = Path.Combine(path, YoutubeDl);
                    var fn = new FileInfo(res);
                    if (fn.Exists)
                    {
                        YouPath = fn.FullName;
                        await youpath.UpdateSettingAsync(fn.FullName);
                    }
                }

                var creds = await GetCredListAsync();
                foreach (var cred in creds)
                {
                    SupportedCreds.Add(cred);
                }

                if (SupportedCreds.Any())
                {
                    SelectedCred = SupportedCreds.First();
                }

                var lsttags = await GetAllTagsAsync();
                foreach (ITag tag in lsttags)
                {
                    Tags.Add(tag);
                }
            }
            catch (Exception ex)
            {
                Info = ex.Message;
                SetStatus(3);
            }
        }

        public async Task SyncChannel(IChannel channel)
        {
            Info = "Syncing: " + channel.Title;
            IsIdle = false;
            var watch = Stopwatch.StartNew();
            try
            {
                await channel.SyncChannelAsync(DirPath, true);
                watch.Stop();
                Info = string.Format("Time: {0} sec", watch.Elapsed.Seconds);
                SetStatus(2);
                IsIdle = true;
            }
            catch (Exception ex)
            {
                IsIdle = true;
                SetStatus(3);
                Info = ex.Message;
                MessageBox.Show(ex.Message);
            }
        }

        public async Task FindRelatedChannels(IChannel channel)
        {
            if (channel == null)
            {
                return;
            }

            SetStatus(1);

            RelatedChannels.Clear();

            var related = await _yf.GetRelatedChannelsByIdAsync(channel.ID);

            foreach (var ch in related.Select(poco => _cf.CreateChannel(poco)))
            {
                if (Channels.Select(x => x.ID).Contains(ch.ID))
                {
                    ch.IsDownloading = true;
                }

                RelatedChannels.Add(ch);
            }

            SetStatus(0);
        }

        public async Task AddNewChannelAsync(string channelid, string channeltitle)
        {
            var channel = await _cf.GetChannelNetAsync(channelid);
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
            channel.IsDownloading = true;
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
                    var lvlite = await _yf.GetVideosListByIdsLiteAsync(nchank);

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
                    var lvfull = await _yf.GetVideosListByIdsAsync(truchank); // ну и начнем получать уже полные объекты

                    foreach (var poco in lvfull)
                    {
                        var vi = _vf.CreateVideoItem(poco);
                        channel.AddNewItem(vi, false);
                        await vi.InsertItemAsync();
                    }
                }
            }

            SetStatus(2);
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

            var regex = new Regex(_youRegex);
            var match = regex.Match(Link);
            if (match.Success)
            {
                var id = match.Groups[1].Value;
                var vi = await _vf.GetVideoItemNetAsync(id);
                vi.ParentID = null;
                SelectedVideoItem = vi;

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
            SetStatus(1);
            Info = string.Empty;
            const string youUser = "user";
            const string youChannel = "channel";
            var parsedChannelId = string.Empty;

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
                    parsedChannelId = await _cf.GetChannelIdByUserNameNetAsync(user);
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
                    parsedChannelId = await _cf.GetChannelIdByUserNameNetAsync(inputChannelId);
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

        public async Task Search()
        {
            if (string.IsNullOrEmpty(SearchKey))
            {
                return;
            }

            SetStatus(1);
            try
            {
                var lst = await _yf.SearchItemsAsync(SearchKey, SelectedCountry, 50);
                if (lst.Any())
                {
                    var channel = ServiceChannels.First();
                    for (var i = channel.ChannelItems.Count; i > 0; i--)
                    {
                        if (!(channel.ChannelItems[i - 1].ItemState == "LocalYes" ||
                              channel.ChannelItems[i - 1].ItemState == "Downloading"))
                        {
                            channel.ChannelItems.RemoveAt(i - 1);
                        }
                    }
                    foreach (var poco in lst)
                    {
                        var item = _vf.CreateVideoItem(poco);
                        channel.AddNewItem(item, false);
                        item.IsHasLocalFileFound(DirPath);
                    }
                    SelectedChannel = channel;
                }
                SetStatus(2);
            }
            catch (Exception ex)
            {
                SetStatus(3);
                Info = ex.Message;
            }
        }

        private async Task EditChannel()
        {
            SelectedChannel.Title = NewChannelTitle;
            await SelectedChannel.RenameChannelAsync(NewChannelTitle);
        }

        public async Task<IEnumerable<string>> GetChannelsIdsListDbAsync()
        {
            try
            {
                return await _df.GetChannelsIdsListDbAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IChannel>> GetChannelsListAsync()
        {
            var lst = new List<IChannel>();
            try
            {
                var fbres = await _df.GetChannelsListAsync();
                lst.AddRange(fbres.Select(poco => _cf.CreateChannel(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ICred>> GetCredListAsync()
        {
            var lst = new List<ICred>();
            try
            {
                var fbres = await _df.GetCredListAsync();
                lst.AddRange(fbres.Select(poco => _crf.CreateCred(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ITag>> GetAllTagsAsync()
        {
            var lst = new List<ITag>();

            try
            {
                var fbres = await _df.GetAllTagsAsync();
                lst.AddRange(fbres.Select(poco => BaseFactory.CreateTagFactory().CreateTag(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void CreateServicesChannels()
        {
            var chpop = _cf.CreateChannel();
            chpop.Title = "#Popular";
            chpop.Site = "youtube.com";
            var img = Assembly.GetExecutingAssembly().GetManifestResourceStream("Crawler.Images.pop.png");
            chpop.Thumbnail = SiteHelper.ReadFully(img);
            chpop.ID = "pop";
            ServiceChannels.Add(chpop);
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

        public ICommonFactory BaseFactory { get; private set; }

        public ObservableCollection<IChannel> Channels { get; private set; }

        public ObservableCollection<IChannel> ServiceChannels { get; set; }

        public ObservableCollection<IChannel> RelatedChannels { get; set; }

        public ObservableCollection<ITag> Tags { get; set; } 

        public List<string> Countries { get; set; }

        public IChannel SelectedChannel
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

        public IVideoItem SelectedVideoItem
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

        public IPlaylist SelectedPlaylist
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

        public List<ICred> SupportedCreds { get; set; }

        public ICred SelectedCred
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

        public string SearchKey
        {
            get
            {
                return _searchKey;
            }
            set
            {
                _searchKey = value; 
                OnPropertyChanged();
            }
        }

        public string NewTag
        {
            get { return _newTag; }
            set
            {
                _newTag = value; 
                OnPropertyChanged();
            }
        }

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
