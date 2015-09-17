// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

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
using Interfaces.POCO;
using Microsoft.WindowsAPICodePack.Taskbar;
using Ninject;
using SitesAPI;
using Container = IoC.Container;

namespace Crawler.Models
{
    public sealed class MainWindowModel : INotifyPropertyChanged
    {
        #region Constants

        private const string dbLaunchParam = "db";
        private const string dirLaunchParam = "dir";
        private const string mpcLaunchParam = "mpc";
        private const string pathToDownload = "pathToDownload";
        private const string pathToMpc = "pathToMpc";
        private const string pathToYoudl = "pathToYoudl";
        private const string youLaunchParam = "you";
        private const string youRegex = @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)";
        private const string youtubeDl = "youtube-dl.exe";

        #endregion

        #region Static and Readonly Fields

        public readonly List<IVideoItem> Filterlist = new List<IVideoItem>();
        private readonly IChannelFactory _cf;
        private readonly ICredFactory _crf;
        private readonly ISqLiteDatabase _df;
        private readonly Dictionary<string, string> _launchParam = new Dictionary<string, string>();
        private readonly ISettingFactory _sf;
        private readonly IVideoItemFactory _vf;
        private readonly IYouTubeSite _yf;

        #endregion

        #region Fields

        private string _dirPath;
        private string _filter;
        private string _info;
        private bool _isExpand;
        private bool _isIdle;
        private string _link;
        private string _mpcPath;
        private string _newChannelLink;
        private string _newChannelTitle;
        private string _newTag;
        private double _prValue;
        private string _result;
        private string _searchKey;
        private IChannel _selectedChannel;
        private string _selectedCountry;
        private ICred _selectedCred;
        private IPlaylist _selectedPlaylist;
        private ITag _selectedTag;
        private IVideoItem _selectedVideoItem;
        private string _youHeader;
        private string _youPath;

        #endregion

        #region Constructors

        public MainWindowModel()
        {
            ParseCommandLineArguments();
            Version = CommonExtensions.GetFileVersion(Assembly.GetExecutingAssembly());
            Channels = new ObservableCollection<IChannel>();
            ServiceChannels = new ObservableCollection<IChannel>();
            RelatedChannels = new ObservableCollection<IChannel>();
            SupportedCreds = new List<ICred>();
            Tags = new ObservableCollection<ITag>();
            CurrentTags = new ObservableCollection<ITag>();
            Countries = new List<string> { "RU", "US", "CA", "FR", "DE", "IT", "JP" };
            SelectedCountry = Countries.First();
            IsIdle = true;

            BaseFactory = Container.Kernel.Get<ICommonFactory>();
            _df = BaseFactory.CreateSqLiteDatabase();
            if (_launchParam.Any())
            {
                // через параметры запуска указали путь к своей базе
                string dbpath;
                if (_launchParam.TryGetValue(dbLaunchParam, out dbpath))
                {
                    _df.FileBase = new FileInfo(dbpath);
                }
            }
            _sf = BaseFactory.CreateSettingFactory();
            _cf = BaseFactory.CreateChannelFactory();
            _vf = BaseFactory.CreateVideoItemFactory();
            _yf = BaseFactory.CreateYouTubeSite();
            _crf = BaseFactory.CreateCredFactory();
        }

        #endregion

        #region Properties

        public ICommonFactory BaseFactory { get; private set; }
        public ObservableCollection<IChannel> Channels { get; private set; }
        public IEnumerable<string> Countries { get; set; }
        public ObservableCollection<ITag> CurrentTags { get; private set; }

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

        public string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
                OnPropertyChanged();
                FilterVideos();
            }
        }

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

        public bool IsAudio { get; set; }
        public bool IsEditMode { get; set; }

        public bool IsExpand
        {
            get
            {
                return _isExpand;
            }
            set
            {
                _isExpand = value;
                OnPropertyChanged();
            }
        }

        public bool IsHd { get; set; }

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

        public string NewChannelTitle
        {
            get
            {
                return _newChannelTitle;
            }
            set
            {
                _newChannelTitle = value;
                OnPropertyChanged();
            }
        }

        public string NewTag
        {
            get
            {
                return _newTag;
            }
            set
            {
                _newTag = value;
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

        public ObservableCollection<IChannel> RelatedChannels { get; set; }

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

        public ITag SelectedTag
        {
            get
            {
                return _selectedTag;
            }
            set
            {
                _selectedTag = value;
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

        public ObservableCollection<IChannel> ServiceChannels { get; set; }
        public List<ICred> SupportedCreds { get; set; }
        public ObservableCollection<ITag> Tags { get; set; }
        public string Version { get; set; }

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

        #endregion

        #region Methods

        public async Task AddNewChannel(string inputChannelId)
        {
            SetStatus(1);
            Info = string.Empty;
            const string youUser = "user";
            const string youChannel = "channel";
            string parsedChannelId = string.Empty;

            string[] sp = inputChannelId.Split('/');
            if (sp.Length > 1)
            {
                if (sp.Contains(youUser))
                {
                    int indexuser = Array.IndexOf(sp, youUser);
                    if (indexuser < 0)
                    {
                        throw new Exception("Can't parse url");
                    }

                    string user = sp[indexuser + 1];
                    parsedChannelId = await _cf.GetChannelIdByUserNameNetAsync(user);
                }
                else if (sp.Contains(youChannel))
                {
                    int indexchannel = Array.IndexOf(sp, youChannel);
                    if (indexchannel < 0)
                    {
                        throw new Exception("Can't parse url");
                    }

                    parsedChannelId = sp[indexchannel + 1];
                }
                else
                {
                    var regex = new Regex(youRegex);
                    Match match = regex.Match(inputChannelId);
                    if (match.Success)
                    {
                        string id = match.Groups[1].Value;
                        IVideoItemPOCO vi = await _yf.GetVideoItemLiteNetAsync(id);
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

        public async Task AddNewChannelAsync(string channelid, string channeltitle)
        {
            IChannel channel = await _cf.GetChannelNetAsync(channelid);
            if (string.IsNullOrEmpty(channel.Title))
            {
                throw new Exception("Can't get channel: " + channel.ID);
            }

            if (!string.IsNullOrEmpty(channeltitle))
            {
                channel.Title = channeltitle;
            }

            await channel.DeleteChannelAsync();
            Channels.Add(channel);
            channel.IsDownloading = true;
            channel.IsShowRow = true;
            channel.IsInWork = true;
            SelectedChannel = channel;
            IEnumerable<IVideoItem> lst = await channel.GetChannelItemsNetAsync(0);
            foreach (IVideoItem item in lst)
            {
                channel.AddNewItem(item, false);
            }

            await channel.InsertChannelItemsAsync();
            IEnumerable<IPlaylist> pls = await channel.GetChannelPlaylistsNetAsync();
            foreach (IPlaylist pl in pls)
            {
                channel.ChannelPlaylists.Add(pl);
                await pl.InsertPlaylistAsync();
                IEnumerable<string> plv = await pl.GetPlaylistItemsIdsListNetAsync(); // получим список id плейлиста
                var needcheck = new List<string>();
                foreach (string id in plv)
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

                IEnumerable<List<string>> nchanks = CommonExtensions.SplitList(needcheck);

                // разобьем на чанки по 50, чтоб поменьше дергать ютуб
                var trueids = new List<string>();
                foreach (List<string> nchank in nchanks)
                {
                    IEnumerable<IVideoItemPOCO> lvlite = await _yf.GetVideosListByIdsLiteAsync(nchank);

                    // получим лайтовые объекты, только id и parentid
                    foreach (IVideoItemPOCO poco in lvlite)
                    {
                        // не наши - пофиг, не нужны
                        if (poco.ParentID != channel.ID)
                        {
                            continue;
                        }
                        trueids.Add(poco.ID); // а вот эти с нашего канала - собираем
                    }
                }

                IEnumerable<List<string>> truchanks = CommonExtensions.SplitList(trueids); // опять же разобьем на чанки

                foreach (List<string> truchank in truchanks)
                {
                    IEnumerable<IVideoItemPOCO> lvfull = await _yf.GetVideosListByIdsAsync(truchank);

                    // ну и начнем получать уже полные объекты
                    foreach (IVideoItemPOCO poco in lvfull)
                    {
                        IVideoItem vi = _vf.CreateVideoItem(poco);
                        channel.AddNewItem(vi, false);
                        await vi.InsertItemAsync();
                    }
                }
            }
            channel.IsInWork = false;
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

            var regex = new Regex(youRegex);
            Match match = regex.Match(Link);
            if (match.Success)
            {
                string id = match.Groups[1].Value;
                IVideoItem vi = await _vf.GetVideoItemNetAsync(id);
                vi.ParentID = null;
                SelectedVideoItem = vi;

                SelectedChannel = ServiceChannels.First();
                ServiceChannels.First().AddNewItem(vi, true);
                await vi.DownloadItem(_youPath, DirPath, IsHd, IsAudio);
                vi.IsNewItem = true;
            }
            else
            {
                string param = string.Format("-o {0}\\%(title)s.%(ext)s {1} --no-check-certificate -i --console-title", DirPath, Link);
                await Task.Run(() =>
                {
                    Process process = Process.Start(_youPath, param);
                    if (process != null)
                    {
                        process.Close();
                    }
                });
            }
        }

        public async Task FillChannels()
        {
            await LoadSettings();

            IEnumerable<IChannel> lst = await GetChannelsListAsync(); // все каналы за раз
            foreach (IChannel ch in lst)
            {
                ch.IsShowRow = true;
                Channels.Add(ch);
            }

            if (Channels.Any())
            {
                SelectedChannel = Channels.First();
            }

            CreateServicesChannels();
        }

        public async Task FindRelatedChannels(IChannel channel)
        {
            if (channel == null)
            {
                return;
            }

            SetStatus(1);

            RelatedChannels.Clear();

            IEnumerable<IChannelPOCO> related = await _yf.GetRelatedChannelsByIdAsync(channel.ID);

            foreach (IChannel ch in related.Select(poco => _cf.CreateChannel(poco)))
            {
                if (Channels.Select(x => x.ID).Contains(ch.ID))
                {
                    ch.IsDownloading = true;
                }

                RelatedChannels.Add(ch);
            }

            SetStatus(0);
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
                try
                {
                    await AddNewChannel(NewChannelLink);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public async Task SaveSettings()
        {
            try
            {
                SetStatus(1);
                ISetting savedir = await _sf.GetSettingDbAsync(pathToDownload);
                if (savedir.Value != DirPath)
                {
                    await savedir.UpdateSettingAsync(DirPath);
                }

                ISetting mpcdir = await _sf.GetSettingDbAsync(pathToMpc);
                if (mpcdir.Value != MpcPath)
                {
                    await mpcdir.UpdateSettingAsync(MpcPath);
                }

                ISetting youpath = await _sf.GetSettingDbAsync(pathToYoudl);
                if (youpath.Value != YouPath)
                {
                    await youpath.UpdateSettingAsync(YouPath);
                }

                foreach (ICred cred in SupportedCreds)
                {
                    await cred.UpdateLoginAsync(cred.Login);
                    await cred.UpdatePasswordAsync(cred.Pass);
                }

                foreach (ITag tag in Tags)
                {
                    await tag.InsertTagAsync();
                }

                SetStatus(4);
            }
            catch (Exception ex)
            {
                Info = ex.Message;
                SetStatus(3);
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
                IEnumerable<IVideoItemPOCO> lst = (await _yf.SearchItemsAsync(SearchKey, SelectedCountry, 50)).ToList();
                if (lst.Any())
                {
                    IChannel channel = ServiceChannels.First();
                    for (int i = channel.ChannelItems.Count; i > 0; i--)
                    {
                        if (
                            !(channel.ChannelItems[i - 1].ItemState == "LocalYes"
                              || channel.ChannelItems[i - 1].ItemState == "Downloading"))
                        {
                            channel.ChannelItems.RemoveAt(i - 1);
                        }
                    }
                    foreach (IVideoItemPOCO poco in lst)
                    {
                        IVideoItem item = _vf.CreateVideoItem(poco);
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

        /// <summary>
        ///     0-Ready
        ///     1-Working..
        ///     2-Finished!
        ///     3-Error
        ///     4-Saved
        /// </summary>
        /// <param name="res"></param>
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
            if (res == 4)
            {
                Result = "Saved";
            }
        }

        public void ShowAllChannels()
        {
            foreach (IChannel channel in Channels)
            {
                channel.IsShowRow = true;
            }

            foreach (ITag tag in CurrentTags)
            {
                tag.IsChecked = false;
            }

            SelectedTag = null;
        }

        public async Task SyncChannel(IChannel channel)
        {
            Info = "Syncing: " + channel.Title;
            IsIdle = false;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                await channel.SyncChannelAsync(true);
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

        public async Task SyncData()
        {
            PrValue = 0;
            IsIdle = false;
            SetStatus(1);
            int i = 0;
            TaskbarManager prog = TaskbarManager.Instance;
            prog.SetProgressState(TaskbarProgressBarState.Normal);
            ShowAllChannels();

            foreach (IChannel channel in Channels)
            {
                try
                {
                    i += 1;
                    PrValue = Math.Round((double)(100 * i) / Channels.Count);
                    prog.SetProgressValue((int)PrValue, 100);
                    Info = "Syncing: " + channel.Title;
                    await channel.SyncChannelAsync(false);
                }
                catch (Exception ex)
                {
                    IsIdle = true;
                    SetStatus(3);
                    Info = ex.Message;
                    MessageBox.Show(channel.Title + Environment.NewLine + ex.Message);
                }
            }

            prog.SetProgressState(TaskbarProgressBarState.NoProgress);
            PrValue = 0;
            IsIdle = true;
            SetStatus(2);
            Info = "Total : " + i + ". New : " + Channels.Sum(x => x.CountNew);
        }

        private void CreateServicesChannels()
        {
            IChannel chpop = _cf.CreateChannel();
            chpop.Title = "#Popular";
            chpop.Site = "youtube.com";
            Stream img = Assembly.GetExecutingAssembly().GetManifestResourceStream("Crawler.Images.pop.png");
            chpop.Thumbnail = SiteHelper.ReadFully(img);
            chpop.ID = "pop";
            ServiceChannels.Add(chpop);
        }

        private async Task EditChannel()
        {
            SelectedChannel.Title = NewChannelTitle;
            await SelectedChannel.RenameChannelAsync(NewChannelTitle);
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

                foreach (IVideoItem item in SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = false;
                }

                foreach (IVideoItem item in Filterlist)
                {
                    IVideoItem vid = SelectedChannel.ChannelItems.FirstOrDefault(x => x.ID == item.ID);
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
                    foreach (IVideoItem item in SelectedChannel.ChannelItems.Where(item => item.IsShowRow))
                    {
                        Filterlist.Add(item);
                    }
                }

                foreach (IVideoItem item in SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = false;
                }

                foreach (IVideoItem item in Filterlist)
                {
                    if (!item.Title.ToLower().Contains(Filter.ToLower()))
                    {
                        continue;
                    }
                    IVideoItem vid = SelectedChannel.ChannelItems.FirstOrDefault(x => x.ID == item.ID);
                    if (vid != null)
                    {
                        vid.IsShowRow = true;
                    }
                }
            }
        }

        private async Task<IEnumerable<ITag>> GetAllTagsAsync()
        {
            var lst = new List<ITag>();

            try
            {
                IEnumerable<ITagPOCO> fbres = await _df.GetAllTagsAsync();
                lst.AddRange(fbres.Select(poco => BaseFactory.CreateTagFactory().CreateTag(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<IEnumerable<IChannel>> GetChannelsListAsync()
        {
            var lst = new List<IChannel>();
            try
            {
                IEnumerable<IChannelPOCO> fbres = await _df.GetChannelsListAsync();
                lst.AddRange(fbres.Select(poco => _cf.CreateChannel(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<IEnumerable<ICred>> GetCredListAsync()
        {
            var lst = new List<ICred>();
            try
            {
                IEnumerable<ICredPOCO> fbres = await _df.GetCredListAsync();
                lst.AddRange(fbres.Select(poco => _crf.CreateCred(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task LoadSettings()
        {
            try
            {
                string param;
                if (_launchParam.TryGetValue(dirLaunchParam, out param))
                {
                    var di = new DirectoryInfo(param);
                    if (di.Exists)
                    {
                        DirPath = di.FullName;
                    }
                    else
                    {
                        MessageBox.Show("Check download path");
                        DirPath = string.Empty;
                    }
                }
                else
                {
                    ISetting savedir = await _sf.GetSettingDbAsync(pathToDownload);
                    DirPath = savedir.Value;

                    if (string.IsNullOrEmpty(DirPath))
                    {
                        DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        await savedir.UpdateSettingAsync(DirPath);
                    }
                    else
                    {
                        var di = new DirectoryInfo(DirPath);
                        if (!di.Exists)
                        {
                            DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            await savedir.UpdateSettingAsync(DirPath);
                        }
                    }
                }

                if (_launchParam.TryGetValue(mpcLaunchParam, out param))
                {
                    var fn = new FileInfo(param);
                    if (fn.Exists)
                    {
                        MpcPath = fn.FullName;
                    }
                    else
                    {
                        MessageBox.Show("Check MPC-BE path");
                        MpcPath = string.Empty;
                    }
                }
                else
                {
                    ISetting mpcdir = await _sf.GetSettingDbAsync(pathToMpc);
                    MpcPath = mpcdir.Value;
                    if (!string.IsNullOrEmpty(MpcPath))
                    {
                        var fn = new FileInfo(MpcPath);
                        if (!fn.Exists)
                        {
                            MessageBox.Show("Check MPC-BE path");
                            MpcPath = string.Empty;
                        }
                    }
                }

                if (_launchParam.TryGetValue(youLaunchParam, out param))
                {
                    var fn = new FileInfo(param);
                    if (fn.Exists)
                    {
                        YouPath = fn.FullName;
                    }
                    else
                    {
                        MessageBox.Show("Check youtube-dl path");
                        YouPath = string.Empty;
                    }
                }
                else
                {
                    ISetting youpath = await _sf.GetSettingDbAsync(pathToYoudl);
                    YouPath = youpath.Value;

                    if (string.IsNullOrEmpty(YouPath))
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory;
                        string res = Path.Combine(path, youtubeDl);
                        var fn = new FileInfo(res);
                        if (fn.Exists)
                        {
                            YouPath = fn.FullName;
                            await youpath.UpdateSettingAsync(fn.FullName);
                        }
                    }
                    else
                    {
                        var fn = new FileInfo(YouPath);
                        if (!fn.Exists)
                        {
                            MessageBox.Show("Check youtube-dl path");
                            YouPath = string.Empty;
                        }
                    }
                }

                IEnumerable<ICred> creds = await GetCredListAsync();
                foreach (ICred cred in creds)
                {
                    SupportedCreds.Add(cred);
                }

                if (SupportedCreds.Any())
                {
                    SelectedCred = SupportedCreds.First();
                }

                IEnumerable<ITag> lsttags = await GetAllTagsAsync();
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

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ParseCommandLineArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length <= 1)
            {
                return;
            }
            for (int i = 1; i < args.Length; i++)
            {
                string[] param = args[i].Split('|');
                if (param.Length != 2)
                {
                    continue;
                }
                if (Directory.Exists(param[1]))
                {
                    _launchParam.Add(param[0].TrimStart('/'), param[1]);
                }
                else
                {
                    var fn = new FileInfo(param[1]);
                    if (fn.Exists)
                    {
                        _launchParam.Add(param[0].TrimStart('/'), fn.FullName);
                    }
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
