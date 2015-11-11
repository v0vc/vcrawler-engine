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
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Crawler.ViewModels;
using DataAPI;
using Extensions;
using Interfaces.API;
using Interfaces.Enums;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Microsoft.WindowsAPICodePack.Taskbar;
using Container = IoC.Container;

namespace Crawler.Models
{
    public sealed class MainWindowModel : INotifyPropertyChanged
    {
        #region Constants

        private const string dbLaunchParam = "db";

        #endregion

        #region Static and Readonly Fields

        public readonly List<IVideoItem> Filterlist = new List<IVideoItem>();
        private readonly IChannelFactory _cf;
        private readonly ICredFactory _crf;
        private readonly ISqLiteDatabase _df;
        private readonly ISettingFactory _sf;
        private readonly ITapochekSite _tf;
        private readonly IVideoItemFactory _vf;
        private readonly IYouTubeSite _yf;
        private readonly Dictionary<string, string> launchParam = new Dictionary<string, string>();

        #endregion

        #region Fields

        private string _filter;
        private string _info;
        private bool _isExpand;
        private double _prValue;
        private string _result;
        private string _searchKey;
        private IChannel _selectedChannel;
        private string _selectedCountry;
        private IPlaylist _selectedPlaylist;
        private ITag _selectedTag;
        private IVideoItem _selectedVideoItem;
        private bool isWorking;

        #endregion

        #region Constructors

        public MainWindowModel()
        {
            ParseCommandLineArguments();
            Version = CommonExtensions.GetFileVersion(Assembly.GetExecutingAssembly());
            Channels = new ObservableCollection<IChannel>();
            ServiceChannels = new ObservableCollection<IChannel>();
            RelatedChannels = new ObservableCollection<IChannel>();
            CurrentTags = new ObservableCollection<ITag>();
            Countries = new List<string> { "RU", "US", "CA", "FR", "DE", "IT", "JP" };
            SelectedCountry = Countries.First();
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                BaseFactory = scope.Resolve<ICommonFactory>();
            }

            SettingsViewModel = new SettingsViewModel(BaseFactory);
            _df = BaseFactory.CreateSqLiteDatabase();

            if (launchParam.Any())
            {
                // через параметры запуска указали путь к своей базе
                string dbpath;
                if (launchParam.TryGetValue(dbLaunchParam, out dbpath))
                {
                    _df.FileBase = new FileInfo(dbpath);
                }
            }
            _sf = BaseFactory.CreateSettingFactory();
            _cf = BaseFactory.CreateChannelFactory();
            _vf = BaseFactory.CreateVideoItemFactory();
            _yf = BaseFactory.CreateYouTubeSite();
            _crf = BaseFactory.CreateCredFactory();
            _tf = BaseFactory.CreateTapochekSite();
        }

        #endregion

        #region Properties

        public ICommonFactory BaseFactory { get; private set; }
        public ObservableCollection<IChannel> Channels { get; private set; }
        public IEnumerable<string> Countries { get; set; }
        public ObservableCollection<ITag> CurrentTags { get; private set; }

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

        public bool IsWorking
        {
            get
            {
                return isWorking;
            }
            set
            {
                isWorking = value;
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

        public ObservableCollection<IChannel> RelatedChannels { get; private set; }

        public string Result
        {
            get
            {
                return _result;
            }
            private set
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

        public IList<IChannel> SelectedChannels
        {
            get
            {
                return Channels.Where(x => x.IsSelected).ToList();
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
        public SettingsViewModel SettingsViewModel { get; private set; }

        public string Version { get; private set; }

        #endregion

        #region Methods

        public async Task AddNewChannel(string channelId, string channelTitle, SiteType site)
        {
            SetStatus(1);
            string parsedId = null;

            switch (site)
            {
                case SiteType.YouTube:
                    parsedId = await _yf.ParseChannelLink(channelId);

                    break;

                case SiteType.Tapochek:

                    // парсим с других площадок
                    parsedId = string.Empty;
                    break;
            }

            if (string.IsNullOrEmpty(parsedId))
            {
                Info = "Can't parse url";
                SetStatus(3);
            }
            else
            {
                if (Channels.Select(x => x.ID).Contains(parsedId))
                {
                    MessageBox.Show("Has already");
                    SetStatus(0);
                }
                else
                {
                    await AddNewChannelAsync(parsedId, channelTitle, site);
                }
            }
        }

        public async Task AddNewChannelAsync(string channelid, string channeltitle, SiteType site)
        {
            IChannel channel = await _cf.GetChannelNetAsync(channelid, site);
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

            IEnumerable<IVideoItem> lst = await channel.GetChannelItemsNetAsync(0); // TODO add site
            foreach (IVideoItem item in lst)
            {
                channel.AddNewItem(item, false);
            }
            await channel.InsertChannelItemsAsync();

            IEnumerable<IPlaylist> pls = await channel.GetChannelPlaylistsNetAsync(); // TODO add site

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
            channel.ChannelItemsCount = channel.ChannelItems.Count;
            channel.PlaylistCount = channel.ChannelPlaylists.Count;
            channel.IsInWork = false;
            SetStatus(0);
        }

        public async Task FillChannelItems()
        {
            if (SelectedChannel == null)
            {
                return;
            }

            Filter = string.Empty;
            IsExpand = false;

            if (RelatedChannels.Any() && !RelatedChannels.Contains(SelectedChannel))
            {
                foreach (IChannel channel in RelatedChannels)
                {
                    channel.ChannelItems.Clear();
                }

                RelatedChannels.Clear();
            }

            foreach (IVideoItem item in SelectedChannel.ChannelItems)
            {
                item.IsShowRow = true;
            }

            // есть новые элементы после синхронизации
            bool isHasNewFromSync = SelectedChannel.ChannelItems.Any()
                                    && SelectedChannel.ChannelItems.Count == SelectedChannel.ChannelItems.Count(x => x.IsNewItem);

            // заполняем только если либо ничего нет, либо одни новые
            if ((!SelectedChannel.ChannelItems.Any() & !SelectedChannel.IsDownloading) || isHasNewFromSync)
            {
                if (isHasNewFromSync)
                {
                    List<string> lstnew = SelectedChannel.ChannelItems.Select(x => x.ID).ToList();
                    SelectedChannel.ChannelItems.Clear();
                    await SelectedChannel.FillChannelItemsDbAsync(SettingsViewModel.DirPath, 25, 0);
                    foreach (IVideoItem item in
                        from item in SelectedChannel.ChannelItems from id in lstnew.Where(id => item.ID == id) select item)
                    {
                        item.IsNewItem = true;
                    }
                }
                else
                {
                    await SelectedChannel.FillChannelItemsDbAsync(SettingsViewModel.DirPath, 25, 0);
                }

                if (SelectedChannel.ChannelItems.Any())
                {
                    SelectedChannel.PlaylistCount = await SelectedChannel.GetChannelPlaylistCountDbAsync();
                }
                else
                {
                    // нет в базе = related channel
                    SetStatus(1);
                    SelectedChannel.IsInWork = true;
                    IEnumerable<IVideoItem> lst = await SelectedChannel.GetChannelItemsNetAsync(0);
                    foreach (IVideoItem item in lst)
                    {
                        SelectedChannel.AddNewItem(item, false);
                    }
                    SelectedChannel.IsInWork = false;
                    SetStatus(0);
                }
            }
            Filterlist.Clear();
        }

        public async Task FindRelatedChannels(IChannel channel)
        {
            if (channel == null)
            {
                return;
            }

            SetStatus(1);

            RelatedChannels.Clear();

            IEnumerable<IChannel> lst = await channel.GetRelatedChannelNetAsync(channel.ID, channel.Site);

            foreach (IChannel ch in lst)
            {
                if (Channels.Select(x => x.ID).Contains(ch.ID))
                {
                    ch.IsDownloading = true;
                }
                RelatedChannels.Add(ch);
            }

            SetStatus(0);
        }

        public void OnStartup()
        {
            SetStatus(1);
            using (var bgv = new BackgroundWorker())
            {
                bgv.DoWork += BgvDoWork;
                bgv.RunWorkerCompleted += BgvRunWorkerCompleted;
                bgv.RunWorkerAsync();
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
                            !(channel.ChannelItems[i - 1].State == ItemState.LocalYes
                              || channel.ChannelItems[i - 1].State == ItemState.Downloading))
                        {
                            channel.ChannelItems.RemoveAt(i - 1);
                        }
                    }
                    foreach (IVideoItemPOCO poco in lst)
                    {
                        IVideoItem item = _vf.CreateVideoItem(poco);
                        channel.AddNewItem(item, false);
                        item.IsHasLocalFileFound(SettingsViewModel.DirPath);
                    }
                    SelectedChannel = channel;
                    SelectedChannel.ChannelItemsCount = channel.ChannelItems.Count;
                }
                SetStatus(0);
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
        ///     3-Error
        ///     4-Saved
        /// </summary>
        /// <param name="res"></param>
        public void SetStatus(int res)
        {
            switch (res)
            {
                case 0:
                    Result = "Ready";
                    IsWorking = false;
                    break;
                case 1:
                    Result = "Working..";
                    IsWorking = true;
                    break;
                case 3:
                    Result = "Error";
                    IsWorking = false;
                    break;
                case 4:
                    Result = "Saved";
                    IsWorking = false;
                    break;
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
            SetStatus(1);
            Info = "Syncing: " + channel.Title;
            Stopwatch watch = Stopwatch.StartNew();
            await channel.SyncChannelAsync(true);
            watch.Stop();
            Info = string.Format("Time: {0} sec", watch.Elapsed.Seconds);
            SetStatus(0);
        }

        public async Task SyncData()
        {
            ShowAllChannels();
            PrValue = 0;
            int i = 0;
            SetStatus(1);
            TaskbarManager prog = TaskbarManager.Instance;
            prog.SetProgressState(TaskbarProgressBarState.Normal);

            foreach (IChannel channel in Channels)
            {
                i += 1;
                PrValue = Math.Round((double)(100 * i) / Channels.Count);
                prog.SetProgressValue((int)PrValue, 100);
                Info = "Syncing: " + channel.Title;
                try
                {
                    await channel.SyncChannelAsync(false);
                }
                catch (Exception ex)
                {
                    SetStatus(3);
                    Info = ex.Message;
                    MessageBox.Show(channel.Title + Environment.NewLine + ex.Message);
                }
            }

            prog.SetProgressState(TaskbarProgressBarState.NoProgress);
            PrValue = 0;
            SetStatus(0);
            Info = "Total : " + i + ". New : " + Channels.Sum(x => x.CountNew);
        }

        private void CreateServicesChannels()
        {
            IChannel chpop = _cf.CreateChannel();
            Stream img = Assembly.GetExecutingAssembly().GetManifestResourceStream("Crawler.Images.pop.png");
            chpop.Thumbnail = SiteHelper.ReadFully(img);
            chpop.ID = "pop";
            chpop.Title = "#Popular";
            chpop.Site = SiteType.YouTube;
            ServiceChannels.Add(chpop);
        }

        private async Task FillChannels()
        {
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
                    launchParam.Add(param[0].TrimStart('/'), param[1]);
                }
                else
                {
                    var fn = new FileInfo(param[1]);
                    if (fn.Exists)
                    {
                        launchParam.Add(param[0].TrimStart('/'), fn.FullName);
                    }
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Event Handling

        private void BgvDoWork(object sender, DoWorkEventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync(new Action(async () => await FillChannels()));
        }

        private async void BgvRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                SetStatus(3);
            }
            else
            {
                if (launchParam.Any())
                {
                    SettingsViewModel.LoadSettingsFromLaunchParam(launchParam);
                }
                else
                {
                    await SettingsViewModel.LoadSettingsFromDb();
                }

                await SettingsViewModel.LoadTagsFromDb();
                await SettingsViewModel.LoadCredsFromDb();
                foreach (ICred cred in SettingsViewModel.SupportedCreds)
                {
                    switch (cred.Site)
                    {
                        case SiteType.Tapochek:
                            _tf.Cred = cred;
                            break;

                        case SiteType.YouTube:
                            _yf.Cred = cred;
                            break;

                        case SiteType.RuTracker:

                            break;
                    }
                }
                CreateServicesChannels();
                SetStatus(0);
            }
        }

        #endregion
    }
}
