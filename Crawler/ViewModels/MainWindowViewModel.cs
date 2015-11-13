// This file contains my intellectual property. Release of this file requires prior approval from me.
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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Autofac;
using Crawler.Common;
using Crawler.Views;
using Extensions;
using Interfaces.API;
using Interfaces.Enums;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Microsoft.WindowsAPICodePack.Taskbar;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using Container = IoC.Container;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;

namespace Crawler.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Constants

        private const string dbLaunchParam = "db";

        #endregion

        #region Static and Readonly Fields

        public readonly List<IVideoItem> Filterlist = new List<IVideoItem>();
        private readonly IChannelFactory _cf;
        private readonly ISqLiteDatabase _df;
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
        private IChannel _selectedChannel;
        private IPlaylist _selectedPlaylist;
        private ITag _selectedTag;
        private IVideoItem _selectedVideoItem;
        private RelayCommand addNewItemCommand;
        private RelayCommand channelDoubleClickCommand;
        private RelayCommand channelKeyDownCommand;
        private RelayCommand channelMenuCommand;
        private RelayCommand channelSelectionChangedCommand;
        private RelayCommand currentTagCheckedCommand;
        private RelayCommand currentTagSelectionChangedCommand;
        private RelayCommand fillChannelsCommand;
        private RelayCommand fillDescriptionCommand;
        private bool isHasBeenFocused;
        private bool isWorking;
        private RelayCommand mainMenuCommand;
        private RelayCommand openDescriptionCommand;
        private RelayCommand playlistDoubleClickCommand;
        private RelayCommand playlistExpandCommand;
        private RelayCommand playlistMenuCommand;
        private RelayCommand playlistSelectCommand;
        private RelayCommand popularSelectCommand;
        private RelayCommand scrollChangedCommand;
        private RelayCommand submenuOpenedCommand;
        private RelayCommand syncDataCommand;
        private RelayCommand tagsDropDownOpenedCommand;
        private RelayCommand videoClickCommand;
        private RelayCommand videoDoubleClickCommand;
        private RelayCommand videoItemMenuCommand;

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                BaseFactory = scope.Resolve<ICommonFactory>();
            }
            ParseCommandLineArguments();
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

            SettingsViewModel = new SettingsViewModel(BaseFactory);

            Version = CommonExtensions.GetFileVersion(Assembly.GetExecutingAssembly());
            Channels = new ObservableCollection<IChannel>();
            ServiceChannel = new ServiceChannelViewModel();
            ServiceChannels = new ObservableCollection<ServiceChannelViewModel>();
            RelatedChannels = new ObservableCollection<IChannel>();
            CurrentTags = new ObservableCollection<ITag>();

            // Countries = new List<string> { "RU", "US", "CA", "FR", "DE", "IT", "JP" };
            // SelectedCountry = Countries.First();
            _cf = BaseFactory.CreateChannelFactory();
            _vf = BaseFactory.CreateVideoItemFactory();
            _yf = BaseFactory.CreateYouTubeSite();
            _tf = BaseFactory.CreateTapochekSite();
        }

        #endregion

        #region Properties

        public RelayCommand AddNewItemCommand
        {
            get
            {
                return addNewItemCommand ?? (addNewItemCommand = new RelayCommand(x => AddNewItem()));
            }
        }

        public ICommonFactory BaseFactory { get; private set; }

        public RelayCommand ChannelDoubleClickCommand
        {
            get
            {
                return channelDoubleClickCommand ?? (channelDoubleClickCommand = new RelayCommand(SyncChannel));
            }
        }

        public RelayCommand ChannelKeyDownCommand
        {
            get
            {
                return channelKeyDownCommand ?? (channelKeyDownCommand = new RelayCommand(ChannelKeyDown));
            }
        }

        public RelayCommand ChannelMenuCommand
        {
            get
            {
                return channelMenuCommand ?? (channelMenuCommand = new RelayCommand(ChannelMenuClick));
            }
        }

        public ObservableCollection<IChannel> Channels { get; private set; }

        public RelayCommand ChannelSelectionChangedCommand
        {
            get
            {
                return channelSelectionChangedCommand ?? (channelSelectionChangedCommand = new RelayCommand(FocusRow));
            }
        }

        public RelayCommand CurrentTagCheckedCommand
        {
            get
            {
                return currentTagCheckedCommand ?? (currentTagCheckedCommand = new RelayCommand(x => TagCheck()));
            }
        }

        public ObservableCollection<ITag> CurrentTags { get; private set; }

        public RelayCommand CurrentTagSelectionChangedCommand
        {
            get
            {
                return currentTagSelectionChangedCommand ?? (currentTagSelectionChangedCommand = new RelayCommand(SelectTag));
            }
        }

        public RelayCommand FillChannelsCommand
        {
            get
            {
                return fillChannelsCommand ?? (fillChannelsCommand = new RelayCommand(x => OnStartup()));
            }
        }

        public RelayCommand FillDescriptionCommand
        {
            get
            {
                return fillDescriptionCommand ?? (fillDescriptionCommand = new RelayCommand(FillDescription));
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

        public bool IsSearchExpanded { get; set; }

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

        public RelayCommand MainMenuCommand
        {
            get
            {
                return mainMenuCommand ?? (mainMenuCommand = new RelayCommand(MainMenuClick));
            }
        }

        public RelayCommand OpenDescriptionCommand
        {
            get
            {
                return openDescriptionCommand ?? (openDescriptionCommand = new RelayCommand(OpenDescription));
            }
        }

        public RelayCommand PlaylistDoubleClickCommand
        {
            get
            {
                return playlistDoubleClickCommand ?? (playlistDoubleClickCommand = new RelayCommand(PlaylistDoubleClick));
            }
        }

        public RelayCommand PlaylistExpandCommand
        {
            get
            {
                return playlistExpandCommand ?? (playlistExpandCommand = new RelayCommand(PlaylistExpand));
            }
        }

        public RelayCommand PlaylistMenuCommand
        {
            get
            {
                return playlistMenuCommand ?? (playlistMenuCommand = new RelayCommand(PlaylistMenuClick));
            }
        }

        public RelayCommand PlaylistSelectCommand
        {
            get
            {
                return playlistSelectCommand ?? (playlistSelectCommand = new RelayCommand(SelectPlaylist));
            }
        }

        public RelayCommand PopularSelectCommand
        {
            get
            {
                return popularSelectCommand ?? (popularSelectCommand = new RelayCommand(SelectPopular));
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

        public RelayCommand ScrollChangedCommand
        {
            get
            {
                return scrollChangedCommand ?? (scrollChangedCommand = new RelayCommand(ScrollChanged));
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

        public ServiceChannelViewModel ServiceChannel { get; set; }
        public ObservableCollection<ServiceChannelViewModel> ServiceChannels { get; set; }
        public SettingsViewModel SettingsViewModel { get; private set; }

        public RelayCommand SubmenuOpenedCommand
        {
            get
            {
                return submenuOpenedCommand ?? (submenuOpenedCommand = new RelayCommand(SubmenuOpened));
            }
        }

        public RelayCommand SyncDataCommand
        {
            get
            {
                return syncDataCommand ?? (syncDataCommand = new RelayCommand(async x => await SyncData()));
            }
        }

        public RelayCommand TagsDropDownOpenedCommand
        {
            get
            {
                return tagsDropDownOpenedCommand ?? (tagsDropDownOpenedCommand = new RelayCommand(x => OpenCurrentTags()));
            }
        }

        public string Version { get; private set; }

        public RelayCommand VideoClickCommand
        {
            get
            {
                return videoClickCommand ?? (videoClickCommand = new RelayCommand(RunItemOneClick));
            }
        }

        public RelayCommand VideoDoubleClickCommand
        {
            get
            {
                return videoDoubleClickCommand ?? (videoDoubleClickCommand = new RelayCommand(RunItemDoubleClick));
            }
        }

        public RelayCommand VideoItemMenuCommand
        {
            get
            {
                return videoItemMenuCommand ?? (videoItemMenuCommand = new RelayCommand(VideoItemMenuClick));
            }
        }

        #endregion

        #region Static Methods

        private static async void FillDescription(object obj)
        {
            var video = obj as IVideoItem;
            if (video != null)
            {
                if (string.IsNullOrEmpty(video.Description))
                {
                    await video.FillDescriptionAsync();
                }
            }
            else
            {
                var channel = obj as IChannel;
                if (channel == null)
                {
                    return;
                }
                if (string.IsNullOrEmpty(channel.SubTitle))
                {
                    await channel.FillChannelDescriptionAsync();
                }
            }
        }

        private static ScrollViewer GetScrollbar(DependencyObject dep)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dep); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dep, i);
                if (child is ScrollViewer)
                {
                    return child as ScrollViewer;
                }
                ScrollViewer sub = GetScrollbar(child);
                if (sub != null)
                {
                    return sub;
                }
            }
            return null;
        }

        private static bool IsFfmegExist()
        {
            const string ff = "ffmpeg.exe";
            string values = Environment.GetEnvironmentVariable("PATH");
            return values != null && values.Split(';').Select(path => Path.Combine(path, ff)).Any(File.Exists);
        }

        private static void OpenDescription(object obj)
        {
            var item = obj as IVideoItem;
            if (item == null)
            {
                return;
            }
            var edvm = new EditDescriptionViewModel(item);
            var edv = new EditDescriptionView
            {
                DataContext = edvm, 
                Owner = Application.Current.MainWindow, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            edv.Show();
        }

        private static async void PlaylistExpand(object obj)
        {
            var ch = obj as IChannel;
            if (ch == null)
            {
                return;
            }

            if (ch.ChannelPlaylists.Any())
            {
                return;
            }

            IEnumerable<IPlaylist> pls = await ch.GetChannelPlaylistsDbAsync();
            {
                foreach (IPlaylist pl in pls)
                {
                    ch.ChannelPlaylists.Add(pl);
                }
            }
        }

        private static async void SubmenuOpened(object obj)
        {
            var item = obj as IVideoItem;
            if (item == null)
            {
                return;
            }
            if (!item.Subtitles.Any())
            {
                await item.FillSubtitles();
            }
        }

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
            var i = 0;
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AddNewItem()
        {
            var edvm = new AddChannelViewModel(false, this);
            var addview = new AddChanelView
            {
                DataContext = edvm, 
                Owner = Application.Current.MainWindow, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            addview.ShowDialog();
        }

        private async Task Backup()
        {
            var dlg = new SaveFileDialog
            {
                FileName = "backup_" + DateTime.Now.ToShortDateString(), 
                DefaultExt = ".txt", 
                Filter = @"Text documents (.txt)|*.txt", 
                OverwritePrompt = true
            };
            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                ISqLiteDatabase fb = BaseFactory.CreateSqLiteDatabase();
                List<IChannelPOCO> lst = (await fb.GetChannelsListAsync()).ToList();
                var sb = new StringBuilder();
                foreach (IChannelPOCO poco in lst)
                {
                    sb.Append(poco.Title).Append("|").Append(poco.ID).Append("|").Append(poco.Site).Append(Environment.NewLine);
                }
                try
                {
                    File.WriteAllText(dlg.FileName, sb.ToString().TrimEnd('\r', '\n'));
                    Info = string.Format("{0} channels has been stored", lst.Count);
                    SetStatus(0);
                }
                catch (Exception ex)
                {
                    Info = ex.Message;
                    SetStatus(3);
                }
            }
        }

        private async void ChannelKeyDown(object par)
        {
            if (par == null)
            {
                return;
            }

            var key = (KeyboardKey)par;
            switch (key)
            {
                case KeyboardKey.Delete:
                    await ConfirmDelete();
                    break;

                case KeyboardKey.Enter:
                    await ServiceChannel.Search();
                    break;
            }
        }

        private async void ChannelMenuClick(object param)
        {
            var menu = (ChannelMenuItem)param;
            switch (menu)
            {
                case ChannelMenuItem.Delete:
                    await ConfirmDelete();
                    break;

                case ChannelMenuItem.Edit:
                    EditChannel();
                    break;

                case ChannelMenuItem.Related:
                    await FindRelated();
                    break;

                case ChannelMenuItem.Subscribe:
                    await Subscribe();
                    break;

                case ChannelMenuItem.Tags:
                    OpenTags();
                    break;

                case ChannelMenuItem.Update:
                    await SyncChannelPlaylist();
                    break;
            }
        }

        private async Task ConfirmDelete()
        {
            var sb = new StringBuilder();

            foreach (IChannel channel in SelectedChannels)
            {
                sb.Append(channel.Title).Append(Environment.NewLine);
            }

            if (sb.Length == 0)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show("Delete:" + Environment.NewLine + sb + "?", 
                "Confirm", 
                MessageBoxButton.OKCancel, 
                MessageBoxImage.Information);

            if (result == MessageBoxResult.OK)
            {
                for (int i = SelectedChannels.Count(); i > 0; i--)
                {
                    IChannel channel = SelectedChannels[i - 1];
                    Channels.Remove(channel);
                    await channel.DeleteChannelAsync();
                }

                if (Channels.Any())
                {
                    SelectedChannel = Channels.First();
                }
            }
        }

        private async Task DeleteItems()
        {
            var sb = new StringBuilder();

            foreach (IVideoItem item in SelectedChannel.SelectedItems)
            {
                if (item.IsHasLocalFile & !string.IsNullOrEmpty(item.LocalFilePath))
                {
                    sb.Append(item.Title).Append(Environment.NewLine);
                }
            }

            if (sb.Length == 0)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show("Are you sure to delete:" + Environment.NewLine + sb + "?", 
                "Confirm", 
                MessageBoxButton.OKCancel, 
                MessageBoxImage.Information);

            if (result == MessageBoxResult.OK)
            {
                for (int i = SelectedChannel.SelectedItems.Count; i > 0; i--)
                {
                    IVideoItem item = SelectedChannel.SelectedItems[i - 1];

                    var fn = new FileInfo(item.LocalFilePath);
                    try
                    {
                        fn.Delete();
                        await item.Log(string.Format("Deleted: {0}", item.LocalFilePath));
                        item.LocalFilePath = string.Empty;
                        item.IsHasLocalFile = false;
                        item.State = ItemState.LocalNo;
                        item.Subtitles.Clear();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private async Task DownloadAudio()
        {
            if (!IsYoutubeExist())
            {
                return;
            }

            SelectedChannel.IsDownloading = true;
            await SelectedVideoItem.DownloadItem(SettingsViewModel.YouPath, SettingsViewModel.DirPath, false, true);
        }

        private async Task DownloadHd()
        {
            if (!IsYoutubeExist())
            {
                return;
            }

            if (IsFfmegExist())
            {
                SelectedChannel.IsDownloading = true;
                await SelectedVideoItem.DownloadItem(SettingsViewModel.YouPath, SettingsViewModel.DirPath, true, false);
            }
            else
            {
                var ff = new FfmpegView
                {
                    Owner = Application.Current.MainWindow, 
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                ff.ShowDialog();
            }
        }

        private void EditChannel()
        {
            var edvm = new AddChannelViewModel(true, this);
            var addview = new AddChanelView
            {
                DataContext = edvm, 
                Owner = Application.Current.MainWindow, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            addview.ShowDialog();
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

        private async Task FindRelated()
        {
            try
            {
                await FindRelatedChannels(SelectedChannel);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                SetStatus(0);
            }
        }

        private async void FocusRow(object obj)
        {
            await FillChannelItems();

            if (isHasBeenFocused)
            {
                return;
            }

            // focus
            var datagrid = obj as DataGrid;
            if (datagrid == null || datagrid.SelectedIndex < 0)
            {
                return;
            }
            datagrid.UpdateLayout();
            var selectedRow = (DataGridRow)datagrid.ItemContainerGenerator.ContainerFromIndex(datagrid.SelectedIndex);
            if (selectedRow == null)
            {
                return;
            }
            selectedRow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            isHasBeenFocused = true;
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

        private bool IsMpcExist()
        {
            if (!string.IsNullOrEmpty(SettingsViewModel.MpcPath))
            {
                return true;
            }
            MessageBox.Show("Please, select MPC");
            return false;
        }

        private bool IsYoutubeExist()
        {
            if (!string.IsNullOrEmpty(SettingsViewModel.YouPath))
            {
                return true;
            }
            MessageBox.Show("Please, select youtube-dl");
            return false;
        }

        private async void MainMenuClick(object param)
        {
            var window = param as Window;
            if (window != null)
            {
                window.Close();
            }
            else
            {
                var menu = (MainMenuItem)param;
                switch (menu)
                {
                    case MainMenuItem.Backup:
                        await Backup();
                        break;

                    case MainMenuItem.Restore:
                        await Restore();
                        break;

                    case MainMenuItem.Settings:
                        OpenSettings();
                        break;

                    case MainMenuItem.Vacuum:
                        await Vacuumdb();
                        break;

                    case MainMenuItem.ShowAll:
                        ShowAllChannels();
                        break;

                    case MainMenuItem.Link:
                        OpenAddLink();
                        break;

                    case MainMenuItem.About:
                        MessageBox.Show("by v0v © 2015", "About", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }
            }
        }

        private void OpenAddLink()
        {
            var dlvm = new DownloadLinkViewModel(this);

            var adl = new DownloadLinkView
            {
                DataContext = dlvm, 
                Owner = Application.Current.MainWindow, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            adl.ShowDialog();
        }

        private async void OpenCurrentTags()
        {
            if (CurrentTags.Any())
            {
                return;
            }

            var tmptags = new List<ITag>();
            foreach (IChannel ch in Channels)
            {
                IEnumerable<ITag> tags = await ch.GetChannelTagsAsync();

                foreach (ITag tag in tags)
                {
                    ch.ChannelTags.Add(tag);
                    if (!tmptags.Select(x => x.Title).Contains(tag.Title))
                    {
                        tmptags.Add(tag);
                    }
                }
            }
            foreach (ITag tag in tmptags.OrderBy(x => x.Title))
            {
                CurrentTags.Add(tag);
            }
        }

        private void OpenSettings()
        {
            var set = new SettingsView
            {
                DataContext = SettingsViewModel, 
                Owner = Application.Current.MainWindow, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            set.ShowDialog();
        }

        private void OpenTags()
        {
            var etvm = new EditTagsViewModel
            {
                ParentChannel = SelectedChannel, 
                CurrentTags = CurrentTags, 
                Tags = SettingsViewModel.SupportedTags, 
                Channels = Channels
            };

            var etv = new EditTagsView
            {
                DataContext = etvm, 
                Owner = Application.Current.MainWindow, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner, 
                Title = string.Format("Tags: {0}", etvm.ParentChannel.Title)
            };
            etv.ShowDialog();
        }

        private void ParseCommandLineArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length <= 1)
            {
                return;
            }
            for (var i = 1; i < args.Length; i++)
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

        private async void PlaylistDoubleClick(object obj)
        {
            var pl = obj as IPlaylist;
            if (pl == null)
            {
                return;
            }

            SetStatus(1);

            IEnumerable<string> pls = await pl.GetPlaylistItemsIdsListNetAsync();

            IVideoItemFactory vf = BaseFactory.CreateVideoItemFactory();

            pl.PlaylistItems.Clear();

            foreach (IVideoItem item in SelectedChannel.ChannelItems)
            {
                item.IsShowRow = false;
            }

            foreach (string id in pls.Where(id => !pl.PlaylistItems.Select(x => x.ID).Contains(id)))
            {
                IVideoItem vi = await vf.GetVideoItemNetAsync(id, pl.Site);

                SelectedChannel.AddNewItem(vi, false);

                pl.PlaylistItems.Add(vi);
            }

            SetStatus(0);
        }

        private void PlaylistLinkToClipboard()
        {
            try
            {
                string link = string.Format("https://www.youtube.com/playlist?list={0}", SelectedPlaylist.ID);
                Clipboard.SetText(link);
            }
            catch (Exception ex)
            {
                Info = ex.Message;
            }
        }

        private async void PlaylistMenuClick(object param)
        {
            var menu = (PlaylistMenuItem)param;
            switch (menu)
            {
                case PlaylistMenuItem.Link:
                    PlaylistLinkToClipboard();
                    break;

                case PlaylistMenuItem.Download:
                    try
                    {
                        await SelectedPlaylist.DownloadPlaylist();
                    }
                    catch (Exception ex)
                    {
                        Info = ex.Message;
                    }

                    break;
            }
        }

        private async Task Restore()
        {
            Info = string.Empty;

            var opf = new OpenFileDialog { Filter = @"Text documents (.txt)|*.txt" };
            DialogResult res = opf.ShowDialog();

            if (res == DialogResult.OK)
            {
                string[] lst = { };

                try
                {
                    lst = File.ReadAllLines(opf.FileName);
                }
                catch (Exception ex)
                {
                    Info = ex.Message;
                    SetStatus(3);
                }

                SetStatus(1);
                TaskbarManager prog = TaskbarManager.Instance;
                prog.SetProgressState(TaskbarProgressBarState.Normal);
                ShowAllChannels();
                var rest = 0;
                foreach (string s in lst)
                {
                    string[] sp = s.Split('|');
                    if (sp.Length == 3)
                    {
                        if (Channels.Select(x => x.ID).Contains(sp[1]))
                        {
                            continue;
                        }

                        switch (sp[2])
                        {
                            case "youtube.com":

                                try
                                {
                                    SetStatus(1);
                                    Info = "Restoring: " + sp[0];
                                    await AddNewChannelAsync(sp[1], null, SiteType.YouTube);
                                }
                                catch (Exception ex)
                                {
                                    SetStatus(3);
                                    Info = "Can't restore: " + sp[0];
                                    MessageBox.Show(ex.Message);
                                }

                                rest++;
                                PrValue = Math.Round((double)(100 * rest) / lst.Count());
                                prog.SetProgressValue((int)PrValue, 100);
                                break;

                            default:

                                Info = "Unsupported site: " + sp[2];

                                break;
                        }
                    }
                    else
                    {
                        Info = "Check: " + s;
                    }
                }

                prog.SetProgressState(TaskbarProgressBarState.NoProgress);
                PrValue = 0;
                SetStatus(0);
                Info = "Total restored: " + rest;
            }
        }

        private async void RunItemDoubleClick(object obj)
        {
            var item = obj as IVideoItem;
            if (item == null)
            {
                return;
            }
            if (IsMpcExist())
            {
                await item.RunItem(SettingsViewModel.MpcPath);
            }
        }

        private async void RunItemOneClick(object obj)
        {
            var item = obj as IVideoItem;
            if (item == null)
            {
                return;
            }
            if (item.IsHasLocalFile)
            {
                if (IsMpcExist())
                {
                    await item.RunItem(SettingsViewModel.MpcPath);
                }
            }
            else
            {
                if (!IsYoutubeExist())
                {
                    return;
                }
                var fn = new FileInfo(SettingsViewModel.YouPath);
                if (!fn.Exists)
                {
                    return;
                }
                SelectedChannel.IsDownloading = true;
                await item.DownloadItem(fn.FullName, SettingsViewModel.DirPath, false, false);
            }
        }

        private async void ScrollChanged(object obj)
        {
            var grid = obj as DataGrid;
            if (grid == null)
            {
                return;
            }
            ScrollViewer scroll = GetScrollbar(grid);
            if (scroll == null)
            {
                return;
            }
            if (scroll.VerticalOffset <= 0)
            {
                return;
            }
            if (SelectedChannel.ChannelItemsCount > SelectedChannel.ChannelItems.Count)
            {
                await
                    SelectedChannel.FillChannelItemsDbAsync(SettingsViewModel.DirPath, 
                        SelectedChannel.ChannelItemsCount - SelectedChannel.ChannelItems.Count, 
                        SelectedChannel.ChannelItems.Count);
            }
        }

        private async void SelectPlaylist(object obj)
        {
            var pl = obj as IPlaylist;
            if (pl == null)
            {
                return;
            }

            IEnumerable<string> lstv = (await pl.GetPlaylistItemsIdsListDbAsync()).ToList();
            if (!lstv.Any())
            {
                foreach (IVideoItem item in SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = false;
                }

                foreach (IVideoItem item in SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = pl.PlaylistItems.Select(x => x.ID).Contains(item.ID);
                }

                return;
            }

            pl.PlaylistItems.Clear();

            foreach (IVideoItem item in SelectedChannel.ChannelItems)
            {
                item.IsShowRow = lstv.Contains(item.ID);
                if (item.IsShowRow)
                {
                    pl.PlaylistItems.Add(item);
                }
            }

            Filterlist.Clear();
        }

        private void SelectPopular(object obj)
        {
            var item = obj as IChannel;
            if (item != null)
            {
                SelectedChannel = item;
            }
        }

        private void SelectTag(object obj)
        {
            var tag = obj as ITag;
            if (tag == null)
            {
                return;
            }

            foreach (IChannel channel in Channels)
            {
                channel.IsShowRow = true;
                if (!channel.ChannelTags.Select(x => x.Title).Contains(tag.Title))
                {
                    channel.IsShowRow = false;
                }
            }

            foreach (ITag item in CurrentTags)
            {
                if (item.Title != tag.Title && item.IsChecked)
                {
                    item.IsChecked = false;
                }
            }
        }

        private async Task Subscribe()
        {
            if (SelectedChannel.IsInWork)
            {
                return;
            }

            Channels.Add(SelectedChannel);
            await SelectedChannel.InsertChannelItemsAsync();
        }

        private async Task SubscribeOn()
        {
            if (SelectedChannel.ID != "pop")
            {
                // этот канал по-любому есть - даже проверять не будем)
                MessageBox.Show("Has already");
                return;
            }

            await AddNewChannel(SelectedVideoItem.MakeLink(), string.Empty, SelectedChannel.Site);
        }

        private async void SyncChannel(object obj)
        {
            var channel = obj as IChannel;
            if (channel != null)
            {
                await SyncChannel(channel);
            }
        }

        private async Task SyncChannelPlaylist()
        {
            SetStatus(1);
            if (SelectedChannel.IsInWork)
            {
                return;
            }

            try
            {
                await SelectedChannel.SyncChannelPlaylistsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                SetStatus(3);
            }

            SetStatus(0);
        }

        private void TagCheck()
        {
            foreach (IChannel channel in Channels)
            {
                channel.IsShowRow = false;
            }

            if (CurrentTags.Any(x => x.IsChecked))
            {
                foreach (ITag tag in CurrentTags.Where(x => x.IsChecked))
                {
                    foreach (IChannel channel in Channels)
                    {
                        if (channel.ChannelTags.Select(x => x.Title).Contains(tag.Title))
                        {
                            channel.IsShowRow = true;
                        }
                    }
                }
            }
            else
            {
                SelectedTag = null;
                foreach (IChannel channel in Channels)
                {
                    channel.IsShowRow = true;
                }
            }
            SelectedChannel = Channels.First(x => x.IsShowRow);
        }

        private async Task Vacuumdb()
        {
            ISqLiteDatabase db = BaseFactory.CreateSqLiteDatabase();
            long sizebefore = db.FileBase.Length;
            await db.VacuumAsync();
            long sizeafter = new FileInfo(db.FileBase.FullName).Length;
            Info = string.Format("Database compacted (bytes): {0} -> {1}", sizebefore, sizeafter);
        }

        private async void VideoItemMenuClick(object param)
        {
            var menu = (VideoMenuItem)param;
            switch (menu)
            {
                case VideoMenuItem.Delete:
                    await DeleteItems();
                    break;

                case VideoMenuItem.Audio:
                    await DownloadAudio();
                    break;

                case VideoMenuItem.Edit:
                    OpenDescription(SelectedVideoItem);
                    break;

                case VideoMenuItem.HD:
                    await DownloadHd();
                    break;

                case VideoMenuItem.Link:
                    VideoLinkToClipboard();
                    break;

                case VideoMenuItem.Subscribe:
                    await SubscribeOn();
                    break;
            }
        }

        private void VideoLinkToClipboard()
        {
            try
            {
                Clipboard.SetText(SelectedVideoItem.MakeLink());
            }
            catch (Exception ex)
            {
                Info = ex.Message;
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
                ServiceChannel.Init(this);
                ServiceChannels.Add(ServiceChannel);
                SetStatus(0);
            }
        }

        #endregion
    }
}
