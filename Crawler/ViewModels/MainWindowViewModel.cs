// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections;
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
using System.Windows.Data;
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
using Models.BO.Channels;
using Models.BO.Items;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using Container = IoC.Container;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;

namespace Crawler.ViewModels
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Constants

        private const string dbLaunchParam = "db";
        private const string txtfilter = "Text documents (.txt)|*.txt";

        #endregion

        #region Static and Readonly Fields

        private readonly IChannelFactory cf;
        private readonly ICollectionView channelCollectionView;
        private readonly ISqLiteDatabase df;
        private readonly Dictionary<string, string> launchParam = new Dictionary<string, string>();
        private readonly ITapochekSite tf;
        private readonly IVideoItemFactory vf;
        private readonly IYouTubeSite yf;

        #endregion

        #region Fields

        private RelayCommand addNewItemCommand;
        private RelayCommand channelDoubleClickCommand;
        private DataGrid channelGrid;
        private RelayCommand channelGridFocusCommand;
        private RelayCommand channelKeyDownCommand;
        private RelayCommand channelMenuCommand;
        private RelayCommand channelSelectCommand;
        private RelayCommand currentTagCheckedCommand;
        private RelayCommand fillChannelsCommand;
        private RelayCommand fillDescriptionCommand;
        private RelayCommand fillSubitlesCommand;
        private string filterChannelKey;
        private string info;
        private bool isExpand;
        private bool isWorking;
        private RelayCommand mainMenuCommand;
        private RelayCommand openDescriptionCommand;
        private RelayCommand playlistDoubleClickCommand;
        private RelayCommand playlistExpandCommand;
        private RelayCommand playlistMenuCommand;
        private RelayCommand playlistSelectCommand;
        private IEnumerable<string> plids;
        private RelayCommand popularSelectCommand;
        private double prValue;
        private string result;
        private RelayCommand scrollChangedCommand;
        private IChannel selectedChannel;
        private IList selectedChannels = new ArrayList();
        private IPlaylist selectedPlaylist;
        private ITag selectedTag;
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
            df = BaseFactory.CreateSqLiteDatabase();
            if (launchParam.Any())
            {
                // через параметры запуска указали путь к своей базе
                string dbpath;
                if (launchParam.TryGetValue(dbLaunchParam, out dbpath))
                {
                    df.FileBase = new FileInfo(dbpath);
                }
            }

            SettingsViewModel = new SettingsViewModel(BaseFactory);

            Version = CommonExtensions.GetFileVersion(Assembly.GetExecutingAssembly());
            Channels = new ObservableCollection<IChannel>();
            channelCollectionView = CollectionViewSource.GetDefaultView(Channels);
            ServiceChannel = new ServiceChannelViewModel();
            ServiceChannels = new ObservableCollection<ServiceChannelViewModel>();
            RelatedChannels = new ObservableCollection<IChannel>();
            CurrentTags = new ObservableCollection<ITag>();

            cf = BaseFactory.CreateChannelFactory();
            vf = BaseFactory.CreateVideoItemFactory();
            yf = BaseFactory.CreateYouTubeSite();
            tf = BaseFactory.CreateTapochekSite();
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

        public RelayCommand ChannelGridFocusCommand
        {
            get
            {
                return channelGridFocusCommand ?? (channelGridFocusCommand = new RelayCommand(FocusRow));
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

        public RelayCommand ChannelSelectCommand
        {
            get
            {
                return channelSelectCommand ?? (channelSelectCommand = new RelayCommand(FillChannelItems));
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

        public RelayCommand FillChannelsCommand
        {
            get
            {
                return fillChannelsCommand ?? (fillChannelsCommand = new RelayCommand(OnStartup));
            }
        }

        public RelayCommand FillDescriptionCommand
        {
            get
            {
                return fillDescriptionCommand ?? (fillDescriptionCommand = new RelayCommand(FillDescription));
            }
        }

        public RelayCommand FillSubitlesCommand
        {
            get
            {
                return fillSubitlesCommand ?? (fillSubitlesCommand = new RelayCommand(FillSubtitles));
            }
        }

        public string FilterChannelKey
        {
            get
            {
                return filterChannelKey;
            }
            set
            {
                if (value == filterChannelKey)
                {
                    return;
                }
                filterChannelKey = value;
                channelCollectionView.Filter = FilterChannel;
                OnPropertyChanged();
            }
        }

        public string Info
        {
            get
            {
                return info;
            }
            private set
            {
                info = value;
                OnPropertyChanged();
            }
        }

        public bool IsExpand
        {
            get
            {
                return isExpand;
            }
            set
            {
                isExpand = value;
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
                return prValue;
            }
            private set
            {
                prValue = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IChannel> RelatedChannels { get; private set; }

        public string Result
        {
            get
            {
                return result;
            }
            private set
            {
                result = value;
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
                return selectedChannel;
            }
            set
            {
                selectedChannel = value;
                OnPropertyChanged();
            }
        }

        public IList SelectedChannels
        {
            get
            {
                return selectedChannels;
            }
            set
            {
                selectedChannels = value;
                OnPropertyChanged();
            }
        }

        public IPlaylist SelectedPlaylist
        {
            get
            {
                return selectedPlaylist;
            }
            set
            {
                selectedPlaylist = value;
                OnPropertyChanged();
            }
        }

        public ITag SelectedTag
        {
            get
            {
                return selectedTag;
            }
            set
            {
                selectedTag = value;
                channelCollectionView.Filter = FilterChannelsByTag;
                OnPropertyChanged();
            }
        }

        public ServiceChannelViewModel ServiceChannel { get; set; }
        public ObservableCollection<ServiceChannelViewModel> ServiceChannels { get; set; }
        public SettingsViewModel SettingsViewModel { get; private set; }

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
                var channel = obj as YouChannel;
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

        private static async void FillSubtitles(object obj)
        {
            var item = obj as YouTubeItem;
            if (item == null)
            {
                return;
            }
            await item.FillSubtitles();
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
            var ch = obj as YouChannel;
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

        #endregion

        #region Methods

        public async Task AddNewChannel(string channelId, string channelTitle, SiteType site)
        {
            SetStatus(1);
            string parsedId = null;

            switch (site)
            {
                case SiteType.YouTube:
                    parsedId = await yf.ParseChannelLink(channelId);

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

        public bool IsYoutubeExist()
        {
            if (!string.IsNullOrEmpty(SettingsViewModel.YouPath))
            {
                return true;
            }
            MessageBox.Show("Please, select youtube-dl");
            return false;
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
                    Info = string.Empty;
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

        private async Task AddNewChannelAsync(string channelid, string channeltitle, SiteType site)
        {
            var channel = (await cf.GetChannelNetAsync(channelid, site)) as YouChannel;
            if (channel == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(channel.Title))
            {
                throw new Exception("Can't get YouChannel: " + channel.ID);
            }

            if (!string.IsNullOrEmpty(channeltitle))
            {
                channel.Title = channeltitle;
            }

            await channel.DeleteChannelAsync();
            Channels.Add(channel);
            channel.IsDownloading = true;
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
                var dicttocheck = new Dictionary<string, IPlaylist>();
                var needcheck = new List<string>();
                foreach (string id in plv)
                {
                    if (channel.ChannelItems.Select(x => x.ID).Contains(id))
                    {
                        await pl.UpdatePlaylistAsync(id); // видео есть на нашем канале - проапдейтим связь
                    }
                    else
                    {
                        if (needcheck.Contains(id))
                        {
                            continue;
                        }
                        needcheck.Add(id); // видео нету - пока добавим в список для дальнейшей проверки
                        dicttocheck.Add(id, pl);
                    }
                }

                IEnumerable<List<string>> nchanks = CommonExtensions.SplitList(needcheck);

                // разобьем на чанки по 50, чтоб поменьше дергать ютуб
                var trueids = new List<string>();
                foreach (List<string> nchank in nchanks)
                {
                    IEnumerable<IVideoItemPOCO> lvlite = await yf.GetVideosListByIdsLiteAsync(nchank);

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
                    IEnumerable<IVideoItemPOCO> lvfull = await yf.GetVideosListByIdsAsync(truchank);

                    // ну и начнем получать уже полные объекты
                    foreach (IVideoItemPOCO poco in lvfull)
                    {
                        IVideoItem vi = vf.CreateVideoItem(poco);
                        channel.AddNewItem(vi, false);
                        await vi.InsertItemAsync();
                        IPlaylist playlist;
                        if (dicttocheck.TryGetValue(vi.ID, out playlist))
                        {
                            // проапдейтим
                            await playlist.UpdatePlaylistAsync(vi.ID);
                        }
                    }
                }
            }
            channel.ChannelItemsCount = channel.ChannelItems.Count;
            channel.PlaylistCount = channel.ChannelPlaylists.Count;
            channel.IsInWork = false;
            SetStatus(0);
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
                Filter = txtfilter, 
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
                    await DeleteChannels();
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
                    await DeleteChannels();
                    break;

                case ChannelMenuItem.Edit:
                    EditChannel();
                    break;

                case ChannelMenuItem.Related:
                    await FindRelated();
                    break;

                case ChannelMenuItem.Subscribe:
                    await SubscribeOnRelated();
                    break;

                case ChannelMenuItem.Tags:
                    OpenTags();
                    break;

                case ChannelMenuItem.Update:
                    await SyncChannelPlaylist();
                    break;
            }
        }

        private async Task DeleteChannels()
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

            MessageBoxResult boxResult = MessageBox.Show("Delete:" + Environment.NewLine + sb + "?", 
                "Confirm", 
                MessageBoxButton.OKCancel, 
                MessageBoxImage.Information);

            if (boxResult == MessageBoxResult.OK)
            {
                for (int i = SelectedChannels.Count; i > 0; i--)
                {
                    var channel = SelectedChannels[i - 1] as YouChannel;
                    if (channel == null)
                    {
                        continue;
                    }
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

            MessageBoxResult boxResult = MessageBox.Show("Are you sure to delete:" + Environment.NewLine + sb + "?", 
                "Confirm", 
                MessageBoxButton.OKCancel, 
                MessageBoxImage.Information);

            if (boxResult == MessageBoxResult.OK)
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

        private async Task DownloadWithOptions(VideoMenuItem item)
        {
            if (!IsYoutubeExist())
            {
                return;
            }
            switch (item)
            {
                case VideoMenuItem.Audio:

                    SelectedChannel.IsDownloading = true;
                    await SelectedChannel.SelectedItem.DownloadItem(SettingsViewModel.YouPath, SettingsViewModel.DirPath, false, true);

                    break;

                case VideoMenuItem.HD:

                    if (IsFfmegExist())
                    {
                        SelectedChannel.IsDownloading = true;
                        await SelectedChannel.SelectedItem.DownloadItem(SettingsViewModel.YouPath, SettingsViewModel.DirPath, true, false);
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

                    break;
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

        private async void FillChannelItems(object obj)
        {
            var channel = obj as YouChannel;
            if (channel == null)
            {
                // ну пока так
                return;
            }

            channel.ChannelItemsCollectionView.Filter = null;

            IsExpand = false;

            if (RelatedChannels.Any() && !RelatedChannels.Contains(channel))
            {
                foreach (IChannel ch in RelatedChannels)
                {
                    ch.ChannelItems.Clear();
                }

                RelatedChannels.Clear();
            }

            // есть новые элементы после синхронизации
            bool isHasNewFromSync = channel.ChannelItems.Any()
                                    && channel.ChannelItems.Count == channel.ChannelItems.Count(x => x.IsNewItem);

            // заполняем только если либо ничего нет, либо одни новые
            if ((!channel.ChannelItems.Any() & !channel.IsDownloading) || isHasNewFromSync)
            {
                if (isHasNewFromSync)
                {
                    List<string> lstnew = channel.ChannelItems.Select(x => x.ID).ToList();
                    channel.ChannelItems.Clear();
                    await channel.FillChannelItemsDbAsync(SettingsViewModel.DirPath, 25, 0);
                    foreach (IVideoItem item in
                        from item in channel.ChannelItems from id in lstnew.Where(id => item.ID == id) select item)
                    {
                        item.IsNewItem = true;
                    }
                }
                else
                {
                    await channel.FillChannelItemsDbAsync(SettingsViewModel.DirPath, 25, 0);
                }

                if (channel.ChannelItems.Any())
                {
                    channel.PlaylistCount = await channel.GetChannelPlaylistCountDbAsync();
                }
                else
                {
                    // нет в базе = related YouChannel
                    SetStatus(1);
                    channel.IsInWork = true;
                    IEnumerable<IVideoItem> lst = await channel.GetChannelItemsNetAsync(0);
                    foreach (IVideoItem item in lst)
                    {
                        channel.AddNewItem(item, false);
                    }
                    channel.IsInWork = false;
                    SetStatus(0);
                }
            }
        }

        private async Task FillChannels()
        {
            IEnumerable<IChannel> lst = await GetChannelsListAsync(); // все каналы за раз
            foreach (IChannel ch in lst)
            {
                Channels.Add(ch);
            }
            if (Channels.Any())
            {
                SelectedChannel = Channels.First();
            }

            FillChannelItems(SelectedChannel);

            // focus
            if (channelGrid == null || channelGrid.SelectedIndex < 0)
            {
                return;
            }
            channelGrid.UpdateLayout();
            var selectedRow = (DataGridRow)channelGrid.ItemContainerGenerator.ContainerFromIndex(channelGrid.SelectedIndex);
            if (selectedRow == null)
            {
                return;
            }
            selectedRow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            channelGrid = null;
        }

        private bool FilterByCheckedTag(object item)
        {
            var channel = (IChannel)item;
            if (channel == null || channel.ChannelTags == null)
            {
                return false;
            }
            if (!CurrentTags.Any(x => x.IsChecked))
            {
                return true;
            }
            bool res = channel.ChannelTags.Any(x => x.IsChecked);
            return res;
        }

        private bool FilterByPlayList(object obj)
        {
            var item = (IVideoItem)obj;
            if (item == null)
            {
                return false;
            }
            bool res = plids.Any(plid => item.ID == plid);
            return res;
        }

        private bool FilterChannel(object item)
        {
            var value = (IChannel)item;
            if (value == null || value.Title == null)
            {
                return false;
            }

            return value.Title.ToLower().Contains(FilterChannelKey.ToLower());
        }

        private bool FilterChannelsByTag(object item)
        {
            var channel = (IChannel)item;
            if (channel == null || channel.ChannelTags == null)
            {
                return false;
            }
            if (SelectedTag == null || string.IsNullOrEmpty(SelectedTag.Title))
            {
                foreach (ITag tag in CurrentTags.Where(tag => tag.IsChecked))
                {
                    tag.IsChecked = false;
                }
                return true;
            }
            if (CurrentTags.Any(x => x.IsChecked))
            {
                return true;
            }
            return channel.ChannelTags.Select(x => x.Title).Contains(SelectedTag.Title);
        }

        private async Task FindRelated()
        {
            try
            {
                var channel = SelectedChannel as YouChannel;
                if (channel != null)
                {
                    await FindRelatedChannels(channel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                SetStatus(0);
            }
        }

        private async Task FindRelatedChannels(YouChannel channel)
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

        private void FocusRow(object obj)
        {
            var channel = obj as YouChannel;
            if (channel != null)
            {
                SelectedChannel = channel;
            }
        }

        private async Task<IEnumerable<IChannel>> GetChannelsListAsync()
        {
            var lst = new List<IChannel>();
            try
            {
                IEnumerable<IChannelPOCO> fbres = await df.GetChannelsListAsync();
                lst.AddRange(fbres.Select(poco => cf.CreateChannel(poco)));
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

                    case MainMenuItem.Link:
                        OpenAddLink();
                        break;

                    case MainMenuItem.About:
                        MessageBox.Show("by v0v © 2015", "About", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }
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

        private void OnStartup(object obj)
        {
            channelGrid = obj as DataGrid;
            SetStatus(1);
            using (var bgv = new BackgroundWorker())
            {
                bgv.DoWork += BgvDoWork;
                bgv.RunWorkerCompleted += BgvRunWorkerCompleted;
                bgv.RunWorkerAsync();
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
            foreach (YouChannel channel in Channels.OfType<YouChannel>())
            {
                IEnumerable<ITag> tags = await channel.GetChannelTagsAsync();

                foreach (ITag tag in tags)
                {
                    channel.ChannelTags.Add(tag);
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
            CurrentTags.Add(BaseFactory.CreateTagFactory().CreateTag()); // empty tag
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
            var channel = SelectedChannel as YouChannel;
            if (channel == null)
            {
                return;
            }
            var etvm = new EditTagsViewModel
            {
                ParentChannel = channel, 
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

            List<string> pls = (await pl.GetPlaylistItemsIdsListNetAsync()).ToList();

            foreach (string id in pls.Where(id => !SelectedChannel.ChannelItems.Select(x => x.ID).Contains(id)))
            {
                IVideoItem vi = await vf.GetVideoItemNetAsync(id, pl.Site);
                SelectedChannel.AddNewItem(vi, false);
                await pl.UpdatePlaylistAsync(id);
            }

            SetStatus(0);
        }

        private void PlaylistLinkToClipboard()
        {
            try
            {
                string link = string.Empty;
                switch (SelectedPlaylist.Site)
                {
                    case SiteType.YouTube:
                        link = string.Format("https://www.youtube.com/playlist?list={0}", SelectedPlaylist.ID);
                        break;
                }

                Clipboard.SetText(link);
            }
            catch
            {
                // ignore
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

            var opf = new OpenFileDialog { Filter = txtfilter };
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

        private async Task RestoreFullChannelItems(YouChannel channel)
        {
            if (channel.ChannelItemsCount > channel.ChannelItems.Count)
            {
                await
                    channel.FillChannelItemsDbAsync(SettingsViewModel.DirPath, 
                        channel.ChannelItemsCount - channel.ChannelItems.Count, 
                        channel.ChannelItems.Count);
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
            var channel = SelectedChannel as YouChannel;
            if (channel == null)
            {
                return;
            }
            await RestoreFullChannelItems(channel);
        }

        private async void SelectPlaylist(object obj)
        {
            var pl = obj as IPlaylist;
            if (pl == null)
            {
                return;
            }

            plids = await pl.GetPlaylistItemsIdsListDbAsync();

            var channel = SelectedChannel as YouChannel;
            if (channel == null)
            {
                return;
            }
            await RestoreFullChannelItems(channel);
            channel.ChannelItemsCollectionView.Filter = FilterByPlayList;
        }

        private void SelectPopular(object obj)
        {
            var item = obj as IChannel;
            if (item != null)
            {
                SelectedChannel = item;
            }
        }

        private async Task SubscribeOnPopular()
        {
            if (SelectedChannel is ServiceChannelViewModel)
            {
                await AddNewChannel(SelectedChannel.SelectedItem.MakeLink(), string.Empty, SelectedChannel.SelectedItem.Site);
            }
        }

        private async Task SubscribeOnRelated()
        {
            if (SelectedChannel.IsInWork)
            {
                return;
            }
            var channel = SelectedChannel as YouChannel;
            if (channel == null)
            {
                return;
            }
            Channels.Add(channel);
            await channel.InsertChannelItemsAsync();
        }

        private async Task SyncChannel(YouChannel channel)
        {
            SetStatus(1);
            Info = "Syncing: " + channel.Title;
            Stopwatch watch = Stopwatch.StartNew();
            await channel.SyncChannelAsync(true);
            watch.Stop();
            Info = string.Format("Time: {0} sec", watch.Elapsed.Seconds);
            SetStatus(0);
        }

        private async void SyncChannel(object obj)
        {
            var channel = obj as YouChannel;
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

            var channel = SelectedChannel as YouChannel;
            if (channel == null)
            {
                return;
            }

            try
            {
                await channel.SyncChannelPlaylistsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                SetStatus(3);
            }

            SetStatus(0);
        }

        private async Task SyncData()
        {
            PrValue = 0;
            var i = 0;
            SetStatus(1);
            TaskbarManager prog = TaskbarManager.Instance;
            prog.SetProgressState(TaskbarProgressBarState.Normal);

            foreach (YouChannel channel in Channels.OfType<YouChannel>())
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

        private void TagCheck()
        {
            channelCollectionView.Filter = FilterByCheckedTag;
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
                    await DownloadWithOptions(menu);
                    break;

                case VideoMenuItem.HD:
                    await DownloadWithOptions(menu);
                    break;

                case VideoMenuItem.Link:
                    VideoLinkToClipboard();
                    break;

                case VideoMenuItem.Subscribe:
                    await SubscribeOnPopular();
                    break;
            }
        }

        private void VideoLinkToClipboard()
        {
            try
            {
                Clipboard.SetText(SelectedChannel.SelectedItem.MakeLink());
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
                            tf.Cred = cred;
                            break;

                        case SiteType.YouTube:
                            yf.Cred = cred;
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
