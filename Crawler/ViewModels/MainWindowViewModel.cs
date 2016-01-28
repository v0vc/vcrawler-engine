// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
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
using Crawler.Common;
using Crawler.Views;
using DataAPI.Database;
using DataAPI.POCO;
using DataAPI.Videos;
using Extensions;
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.Models;
using Microsoft.WindowsAPICodePack.Taskbar;
using Models.BO.Channels;
using Models.BO.Items;
using Models.Factories;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;

namespace Crawler.ViewModels
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Constants

        private const int basePage = 25;
        private const string dbLaunchParam = "db";
        private const string txtfilter = "Text documents (.txt)|*.txt";

        #endregion

        #region Static and Readonly Fields

        private readonly ICollectionView channelCollectionView;
        private readonly SqLiteDatabase df;
        private readonly Dictionary<string, string> launchParam = new Dictionary<string, string>();

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
        private RelayCommand fillRelatedChannelCommand;
        private RelayCommand fillSubitlesCommand;
        private string filterChannelKey;
        private string info;
        private bool isLogExpand;
        private bool isPlExpand;
        private bool isWorking;
        private RelayCommand mainMenuCommand;
        private RelayCommand openDescriptionCommand;
        private RelayCommand playlistExpandCommand;
        private RelayCommand playlistMenuCommand;
        private RelayCommand playlistSelectCommand;
        private RelayCommand popularFillCommand;
        private RelayCommand popularSelectCommand;
        private double prValue;
        private string result;
        private RelayCommand scrollChangedCommand;
        private RelayCommand searchCommand;
        private IChannel selectedChannel;
        private IList selectedChannels = new ArrayList();
        private IList selectedItems = new ArrayList();
        private IPlaylist selectedPlaylist;
        private ITag selectedTag;
        private RelayCommand siteChangedCommand;
        private RelayCommand syncDataCommand;
        private RelayCommand tagsDropDownOpenedCommand;
        private RelayCommand videoClickCommand;
        private RelayCommand videoDoubleClickCommand;
        private RelayCommand videoItemMenuCommand;
        private RelayCommand changeWatchedCommand;

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
            ParseCommandLineArguments();
            df = CommonFactory.CreateSqLiteDatabase();
            if (launchParam.Any())
            {
                // через параметры запуска указали путь к своей базе
                string dbpath;
                if (launchParam.TryGetValue(dbLaunchParam, out dbpath))
                {
                    df.FileBase = new FileInfo(dbpath);
                }
            }

            SettingsViewModel = new SettingsViewModel(UpdateChannelsDir);
            Version = CommonExtensions.GetFileVersion(Assembly.GetExecutingAssembly());
            Channels = new ObservableCollection<IChannel>();
            channelCollectionView = CollectionViewSource.GetDefaultView(Channels);
            ServiceChannels = new ObservableCollection<ServiceChannelViewModel>();
            RelatedChannels = new ObservableCollection<IChannel>();
            CurrentTags = new ObservableCollection<ITag>();
            ServiceChannel = new ServiceChannelViewModel();
            InitBase();
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

        public RelayCommand ChangeWatchedCommand
        {
            get
            {
                return changeWatchedCommand ?? (changeWatchedCommand = new RelayCommand(ChangeWatchState));
            }
        }

        private static void ChangeWatchState(object obj)
        {
            var item = obj as IVideoItem;
            if (item != null)
            {
                item.IsWatched = !item.IsWatched;
            }
        }

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

        public RelayCommand ChannelSelectCommand
        {
            get
            {
                return channelSelectCommand ?? (channelSelectCommand = new RelayCommand(ScrollToTop));
            }
        }

        public ObservableCollection<IChannel> Channels { get; private set; }

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

        public RelayCommand FillRelatedChannelCommand
        {
            get
            {
                return fillRelatedChannelCommand ?? (fillRelatedChannelCommand = new RelayCommand(FillRelated));
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
                channelCollectionView.Filter = FilterChannelByTitleOrId;
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

        public bool IsLogExpand
        {
            get
            {
                return isLogExpand;
            }
            set
            {
                if (value == isLogExpand)
                {
                    return;
                }
                isLogExpand = value;
                OnPropertyChanged();
            }
        }

        public bool IsPlExpand
        {
            get
            {
                return isPlExpand;
            }
            set
            {
                if (value == isPlExpand)
                {
                    return;
                }
                isPlExpand = value;
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

        public RelayCommand PopularFillCommand
        {
            get
            {
                return popularFillCommand ?? (popularFillCommand = new RelayCommand(x => FillPopular()));
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

        public RelayCommand SearchCommand
        {
            get
            {
                return searchCommand ?? (searchCommand = new RelayCommand(async x => await SearchExecute()));
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
                if (Equals(value, selectedChannel))
                {
                    return;
                }
                selectedChannel = value;
                OnPropertyChanged();
                FillChannelItems(selectedChannel);
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
                if (Equals(value, selectedChannels))
                {
                    return;
                }
                selectedChannels = value;
                OnPropertyChanged();
            }
        }

        public IList SelectedItems
        {
            get
            {
                return selectedItems;
            }
            set
            {
                if (Equals(value, selectedItems))
                {
                    return;
                }
                selectedItems = value;
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
                if (Equals(value, selectedPlaylist))
                {
                    return;
                }
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
                if (Equals(value, selectedTag))
                {
                    return;
                }
                selectedTag = value;
                channelCollectionView.Filter = FilterChannelsByTag;
                OnPropertyChanged();
            }
        }

        public ServiceChannelViewModel ServiceChannel { get; set; }
        public ObservableCollection<ServiceChannelViewModel> ServiceChannels { get; set; }

        public RelayCommand SiteChangedCommand
        {
            get
            {
                return siteChangedCommand ?? (siteChangedCommand = new RelayCommand(SiteChanged));
            }
        }

        public RelayCommand SyncDataCommand
        {
            get
            {
                return syncDataCommand ?? (syncDataCommand = new RelayCommand(async x => await SyncData(true)));
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

        private SettingsViewModel SettingsViewModel { get; set; }

        #endregion

        #region Static Methods

        private static void AddDefPlaylist(IChannel channel, List<string> ids)
        {
            Stream img = Assembly.GetExecutingAssembly().GetManifestResourceStream("Crawler.Images.pop.png");
            IPlaylist defpl = PlaylistFactory.CreateUploadPlaylist(channel, ids, StreamHelper.ReadFully(img));
            channel.ChannelPlaylists.Add(defpl);
        }

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
                if (!string.IsNullOrEmpty(channel.SubTitle))
                {
                    return;
                }
                string text = await CommonFactory.CreateSqLiteDatabase().GetChannelDescriptionAsync(channel.ID);
                channel.SubTitle = text.WordWrap(80);
            }
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

            if (ch.ChannelPlaylists.Any() && ch.ChannelPlaylists.Any(x => x.State != SyncState.Added))
            {
                return;
            }

            List<IPlaylist> pls = await ChannelFactory.GetChannelPlaylistsAsync(ch.ID);

            foreach (IPlaylist pl in pls)
            {
                ch.ChannelPlaylists.Add(pl);
            }

            List<string> lst = await CommonFactory.CreateSqLiteDatabase().GetChannelItemsIdListDbAsync(ch.ID, 0, 0);
            AddDefPlaylist(ch, lst);
        }

        private static void ScrollToTop(object obj)
        {
            var vGrid = obj as DataGrid;
            if (vGrid == null)
            {
                return;
            }
            ScrollViewer scr = UiExtensions.GetScrollbar(vGrid);
            if (scr != null)
            {
                scr.ScrollToTop();
            }
        }

        #endregion

        #region Methods

        private void AddItemToDownloadToList(IVideoItem item)
        {
            SelectedChannel = ServiceChannel;
            SelectedChannel.AddNewItem(item);
        }

        private async void AddNewChannel(string channelId, string channelTitle, SiteType site)
        {
            string parsedId = null;

            switch (site)
            {
                case SiteType.YouTube:
                    parsedId = await YouTubeSite.ParseChannelLink(channelId);

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
                IChannel ch = Channels.FirstOrDefault(x => x.ID == parsedId);
                if (ch != null)
                {
                    SelectedChannel = ch;
                    MessageBox.Show("Has already");
                    SetStatus(0);
                }
                else
                {
                    try
                    {
                        await AddNewChannelAsync(parsedId, channelTitle, site);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private async Task AddNewChannelAsync(string channelid, string channeltitle, SiteType site)
        {
            SetStatus(1);

            IChannel channel = await ChannelFactory.GetChannelNetAsync(channelid, site);

            if (channel == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(channeltitle))
            {
                channel.Title = channeltitle;
            }

            channel.DirPath = SettingsViewModel.DirPath;
            Channels.Add(channel);
            SelectedChannel = channel;
            channel.ChannelState = ChannelState.InWork;
            await df.InsertChannelFullAsync(channel);
            AddDefPlaylist(channel, channel.ChannelItems.Select(x => x.ID).ToList());
            channel.ChannelItemsCount = channel.ChannelItems.Count;
            channel.PlaylistCount = channel.ChannelPlaylists.Count;
            channel.ChannelState = ChannelState.Added;
            SetStatus(0);
        }

        private void AddNewItem()
        {
            var edvm = new AddChannelViewModel(false, SettingsViewModel.SupportedCreds, AddNewChannel);
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
                List<ChannelPOCO> lst = await df.GetChannelsListAsync();
                var sb = new StringBuilder();
                foreach (ChannelPOCO poco in lst)
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
                    await SearchExecute();
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
                    try
                    {
                        await SubscribeOnRelated();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    break;

                case ChannelMenuItem.Tags:
                    OpenTags();
                    break;

                case ChannelMenuItem.Update:
                    await SyncChannelPlaylist();
                    break;
            }
        }

        private void ChannelTagDelete(string tag)
        {
            if (Channels.Any(x => x.ChannelTags.Select(y => y.Title).Contains(tag)))
            {
                return;
            }
            ITag ctag = CurrentTags.FirstOrDefault(x => x.Title == tag);
            if (ctag != null)
            {
                CurrentTags.Remove(ctag);
            }
        }

        private async void ChannelTagsSave(IChannel channel)
        {
            if (!CurrentTags.Any())
            {
                foreach (IChannel ch in Channels)
                {
                    List<ITag> tags = await ChannelFactory.GetChannelTagsAsync(ch.ID);
                    foreach (ITag tag in tags)
                    {
                        ch.ChannelTags.Add(tag);
                        if (!CurrentTags.Select(x => x.Title).Contains(tag.Title))
                        {
                            CurrentTags.Add(tag);
                        }
                    }
                }
            }

            foreach (ITag tag in channel.ChannelTags)
            {
                await df.InsertChannelTagsAsync(channel.ID, tag.Title);

                if (!CurrentTags.Select(x => x.Title).Contains(tag.Title))
                {
                    CurrentTags.Insert(0, tag);
                }
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

            MessageBoxResult boxResult = MessageBox.Show(string.Format("Delete:{0}{1}?", Environment.NewLine, sb),
                "Confirm",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information);

            if (boxResult == MessageBoxResult.OK)
            {
                for (int i = SelectedChannels.Count; i > 0; i--)
                {
                    var channel = SelectedChannels[i - 1] as IChannel;
                    if (channel == null)
                    {
                        continue;
                    }

                    Channels.Remove(channel);
                    await df.DeleteChannelAsync(channel.ID);
                    channelCollectionView.Filter = null;
                    FilterChannelKey = string.Empty;
                }

                if (Channels.Any())
                {
                    SelectedChannel = Channels.First();
                }
            }
        }

        private async Task DeleteItems(bool isDeleteFromDbToo = false)
        {
            IChannel channel = SelectedChannel;
            if (channel == null)
            {
                return;
            }

            if (!SelectedItems.OfType<IVideoItem>().Any())
            {
                return;
            }

            if (channel is ServiceChannelViewModel && isDeleteFromDbToo)
            {
                return;
            }

            if (!isDeleteFromDbToo && SelectedItems.OfType<IVideoItem>().All(x => string.IsNullOrEmpty(x.LocalFilePath)))
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (IVideoItem item in SelectedItems)
            {
                if (isDeleteFromDbToo)
                {
                    sb.Append(item.Title).Append(Environment.NewLine);
                }
                else
                {
                    if (item.FileState == ItemState.LocalYes && !string.IsNullOrEmpty(item.LocalFilePath))
                    {
                        sb.Append(item.Title).Append(Environment.NewLine);
                    }
                }
            }

            string res =
                string.Format(
                              isDeleteFromDbToo
                                  ? "Are you sure to delete FROM DB(!){0}Local file will not be deleted:{0}{1}?"
                                  : "Are you sure to delete:{0}{1}?",
                    Environment.NewLine,
                    sb);

            MessageBoxResult boxResult = MessageBox.Show(res, "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Information);

            if (boxResult == MessageBoxResult.OK)
            {
                for (int i = SelectedItems.Count; i > 0; i--)
                {
                    var item = SelectedItems[i - 1] as IVideoItem;
                    if (item == null)
                    {
                        continue;
                    }

                    try
                    {
                        if (isDeleteFromDbToo)
                        {
                            channel.DeleteItem(item);
                            await df.DeleteItemAsync(item.ID);
                        }
                        else
                        {
                            var fn = new FileInfo(item.LocalFilePath);
                            fn.Delete();
                            item.LocalFilePath = string.Empty;
                            item.FileState = ItemState.LocalNo;
                            item.Subtitles.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                if (isDeleteFromDbToo && channel.CountNew >= 0)
                {
                    await CommonFactory.CreateSqLiteDatabase().UpdateChannelNewCountAsync(channel.ID, channel.CountNew);
                }
            }
        }

        private async Task DownloadWithOptions(VideoMenuItem item)
        {
            if (!SettingsViewModel.IsYoutubeExist())
            {
                return;
            }
            var channel = SelectedChannel as YouChannel;
            if (channel == null)
            {
                return;
            }
            switch (item)
            {
                case VideoMenuItem.Audio:

                    channel.ChannelState = ChannelState.InWork;
                    await channel.SelectedItem.DownloadItem(SettingsViewModel.YouPath, SettingsViewModel.DirPath, false, true);
                    channel.ChannelState = ChannelState.HasDownload;
                    break;

                case VideoMenuItem.HD:

                    if (SettingsViewModel.IsFfmegExist())
                    {
                        channel.ChannelState = ChannelState.InWork;
                        await channel.SelectedItem.DownloadItem(SettingsViewModel.YouPath, SettingsViewModel.DirPath, true, false);
                        channel.ChannelState = ChannelState.HasDownload;
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
            var edvm = new AddChannelViewModel(true, SettingsViewModel.SupportedCreds, null, SelectedChannel);
            var addview = new AddChanelView
            {
                DataContext = edvm,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            addview.ShowDialog();
        }

        private async void FillChannelItems(IChannel channel)
        {
            IsLogExpand = false;
            IsPlExpand = false;

            if (channel == null)
            {
                return;
            }

            channel.ChannelItemsCollectionView.Filter = null;

            // если канал только что добавили - элементы там есть
            if (channel.ChannelState == ChannelState.Added)
            {
                return;
            }

            // есть новые элементы после синхронизации - теперь проставляется в самой синхронизации
            // если канал заполнен элементами, но нет новых - уже загружали, не нужно больше
            if (channel.ChannelItems.Any() && !channel.IsHasNewFromSync)
            {
                return;
            }

            // заполняем только если либо ничего нет, либо одни новые
            if (channel.IsHasNewFromSync)
            {
                await
                    ChannelFactory.FillChannelItemsFromDbAsync(channel, basePage - channel.ChannelItems.Count, channel.ChannelItems.Count);
                channel.IsHasNewFromSync = false;
            }
            else
            {
                await ChannelFactory.FillChannelItemsFromDbAsync(channel, basePage, 0);
            }

            if (channel.PlaylistCount == 0)
            {
                channel.PlaylistCount = await df.GetChannelPlaylistCountDbAsync(channel.ID);
            }
        }

        private void FillChannels()
        {
            var fbres = new List<ChannelPOCO>();
            using (var bgv = new BackgroundWorker())
            {
                bgv.DoWork += GetPocoChannelsDoWork;
                bgv.RunWorkerCompleted += GetPocoChannelsCompleted;
                bgv.RunWorkerAsync(fbres);
            }
        }

        private async void FillPopular()
        {
            SetStatus(1);
            var ids = new HashSet<string>(Channels.Select(x => x.ID));
            await ServiceChannel.FillPopular(ids);
            SelectedChannel.ChannelItemsCount = ServiceChannel.ChannelItems.Count;
            SetStatus(0);
        }

        private async void FillRelated(object obj)
        {
            var channel = SelectedChannel as YouChannel;
            if (channel == null)
            {
                return;
            }
            int count = obj == null ? 0 : YouTubeSite.ItemsPerPage;

            if (obj != null && channel.ChannelItems.Any())
            {
                return;
            }
            SetStatus(1);
            channel.ChannelState = ChannelState.InWork;
            IEnumerable<IVideoItem> lst = await ChannelFactory.GetChannelItemsNetAsync(channel, count);
            foreach (IVideoItem item in lst)
            {
                channel.AddNewItem(item);
            }
            channel.ChannelItemsCount = channel.ChannelItems.Count;
            SetStatus(0);
            channel.ChannelState = ChannelState.Notset;
        }

        private async void FillSubtitles(object obj)
        {
            var item = obj as YouTubeItem;
            if (item == null)
            {
                return;
            }
            try
            {
                await item.FillSubtitles();
            }
            catch (Exception ex)
            {
                Info = ex.Message;
            }
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
            if (SelectedPlaylist == null)
            {
                return false;
            }
            if (SelectedPlaylist.IsDefault)
            {
                return true;
            }

            bool res = SelectedPlaylist.PlItems.Any(plid => item.ID == plid);
            return res;
        }

        private bool FilterChannelByTitleOrId(object item)
        {
            var value = (IChannel)item;
            if (value == null || value.Title == null)
            {
                return false;
            }

            string key = FilterChannelKey.ToLower();
            bool res = value.Title.ToLower().Contains(key);
            return res || value.ID.ToLower().Contains(key);
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

        private async Task FindRelatedChannels(IChannel channel)
        {
            if (channel == null)
            {
                return;
            }

            SetStatus(1);

            RelatedChannels.Clear();

            IEnumerable<IChannel> lst = await ChannelFactory.GetRelatedChannelNetAsync(channel);

            foreach (IChannel ch in lst)
            {
                if (Channels.Select(x => x.ID).Contains(ch.ID))
                {
                    ch.ChannelState = ChannelState.Added;
                }
                RelatedChannels.Add(ch);
            }

            SetStatus(0);
        }

        private void FocusRow(object obj)
        {
            var channel = obj as IChannel;
            if (channel != null)
            {
                SelectedChannel = channel;
            }
        }

        private async void InitBase()
        {
            try
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
                ServiceChannel.Init(SettingsViewModel.SupportedCreds, SettingsViewModel.DirPath);
                ServiceChannels.Add(ServiceChannel);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

                    case MainMenuItem.Sync:
                        await SyncData(false);
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
            try
            {
                FillChannels();
                SetStatus(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                SetStatus(3);
            }
        }

        private void OpenAddLink()
        {
            if (!SettingsViewModel.IsYoutubeExist())
            {
                return;
            }

            var dlvm = new DownloadLinkViewModel(SettingsViewModel.YouPath, SettingsViewModel.DirPath, AddItemToDownloadToList);
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
            foreach (IChannel channel in Channels)
            {
                List<ITag> tags = await ChannelFactory.GetChannelTagsAsync(channel.ID);

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
            CurrentTags.Add(TagFactory.CreateTag()); // empty tag
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
            IChannel channel = SelectedChannel;
            if (channel == null)
            {
                return;
            }

            var etvm = new EditTagsViewModel(channel, SettingsViewModel.SupportedTags, ChannelTagDelete, ChannelTagsSave);
            var etv = new EditTagsView
            {
                DataContext = etvm,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = string.Format("Tags: {0}", channel.Title)
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
            if (SelectedPlaylist == null)
            {
                return;
            }
            IPlaylist playlist = SelectedPlaylist;
            var menu = (PlaylistMenuItem)param;
            switch (menu)
            {
                case PlaylistMenuItem.Link:
                    PlaylistLinkToClipboard();
                    break;

                case PlaylistMenuItem.Download:
                    try
                    {
                        await PlaylistFactory.DownloadPlaylist(playlist, SelectedChannel, SettingsViewModel.YouPath);
                    }
                    catch (Exception ex)
                    {
                        Info = ex.Message;
                    }

                    break;

                case PlaylistMenuItem.DownloadHd:
                    try
                    {
                        await PlaylistFactory.DownloadPlaylist(playlist, SelectedChannel, SettingsViewModel.YouPath, true);
                    }
                    catch (Exception ex)
                    {
                        Info = ex.Message;
                    }

                    break;

                case PlaylistMenuItem.Audio:
                    try
                    {
                        await PlaylistFactory.DownloadPlaylist(playlist, SelectedChannel, SettingsViewModel.YouPath, false, true);
                    }
                    catch (Exception ex)
                    {
                        Info = ex.Message;
                    }

                    break;

                case PlaylistMenuItem.Update:

                    if (SelectedChannel == null)
                    {
                        return;
                    }

                    SetStatus(1);
                    try
                    {
                        await PlaylistFactory.UpdatePlaylist(playlist, SelectedChannel);
                        SelectPlaylist(playlist);
                        SetStatus(0);
                    }
                    catch (Exception ex)
                    {
                        SetStatus(3);
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
                int rest = 0;
                HashSet<string> ids = Channels.Select(x => x.ID).ToHashSet();
                foreach (string s in lst)
                {
                    string[] sp = s.Split('|');
                    if (sp.Length == 3)
                    {
                        if (ids.Contains(sp[1]))
                        {
                            continue;
                        }

                        SiteType backupSiteType;
                        if (Enum.TryParse(sp[2], out backupSiteType))
                        {
                            try
                            {
                                switch (backupSiteType)
                                {
                                    case SiteType.YouTube:
                                        SetStatus(1);
                                        Info = "Restoring: " + sp[0];
                                        await AddNewChannelAsync(sp[1], sp[0], backupSiteType);
                                        ids.Add(sp[1]);
                                        break;

                                    default:
                                        MessageBox.Show("Unsupported site: " + sp[2]);
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                SetStatus(3);
                                Info = "Can't restore: " + sp[0];
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        Info = "Check: " + s;
                        MessageBox.Show(s);
                    }

                    rest++;
                    PrValue = Math.Round((double)(100 * rest) / lst.Count());
                    prog.SetProgressValue((int)PrValue, 100);
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
            if (SettingsViewModel.IsMpcExist())
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
            if (item.FileState == ItemState.LocalYes)
            {
                if (SettingsViewModel.IsMpcExist())
                {
                    await item.RunItem(SettingsViewModel.MpcPath);
                }
            }
            else
            {
                if (!SettingsViewModel.IsYoutubeExist())
                {
                    return;
                }
                var fn = new FileInfo(SettingsViewModel.YouPath);
                if (!fn.Exists)
                {
                    return;
                }
                IChannel channel = SelectedChannel;
                if (channel == null)
                {
                    return;
                }
                channel.ChannelState = ChannelState.InWork;
                await item.DownloadItem(fn.FullName, SettingsViewModel.DirPath, false, false);
                channel.ChannelState = ChannelState.HasDownload;
            }
        }

        private void ScrollChanged(object obj)
        {
            var grid = obj as DataGrid;
            if (grid == null)
            {
                return;
            }
            ScrollViewer scroll = UiExtensions.GetScrollbar(grid);
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
            channel.RestoreFullChannelItems();
        }

        private async Task SearchExecute()
        {
            SetStatus(1);
            var ids = new HashSet<string>(Channels.Select(x => x.ID));
            await ServiceChannel.Search(ids);
            SelectedChannel = ServiceChannel;
            SetStatus(0);
        }

        private void SelectPlaylist(object obj)
        {
            var pl = obj as IPlaylist;
            if (pl == null)
            {
                return;
            }

            var channel = SelectedChannel as YouChannel;
            if (channel == null)
            {
                return;
            }
            channel.RestoreFullChannelItems();
            channel.ChannelItemsCollectionView.Filter = FilterByPlayList;
        }

        private void SelectPopular(object obj)
        {
            var item = obj as IChannel;
            if (item == null)
            {
                return;
            }
            SelectedChannel = item;
        }

        /// <summary>
        ///     0-Ready
        ///     1-Working..
        ///     3-Error
        ///     4-Saved
        /// </summary>
        /// <param name="res"></param>
        private void SetStatus(int res)
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

        private void SiteChanged(object obj)
        {
            var item = obj as IChannel;
            if (item != null)
            {
                SelectedChannel = item;
            }

            ServiceChannel.SiteChanged();
        }

        private async void SubscribeOnPopular()
        {
            if (SelectedChannel == null)
            {
                return;
            }
            IChannel channel = SelectedChannel;
            if (channel.SelectedItem == null)
            {
                return;
            }
            IVideoItem item = channel.SelectedItem;
            HashSet<string> ids = Channels.Select(x => x.ID).ToHashSet();
            if (ids.Contains(item.ParentID))
            {
                return;
            }
            await AddNewChannelAsync(item.ParentID, string.Empty, SiteType.YouTube);
            item.SyncState = SyncState.Added;
        }

        private async Task SubscribeOnRelated()
        {
            var channel = SelectedChannel as YouChannel;
            if (channel == null || channel.ChannelState == ChannelState.InWork || Channels.Select(x => x.ID).Contains(channel.ID))
            {
                return;
            }

            Channels.Add(channel);

            IChannel ch = Channels[Channels.IndexOf(channel)];
            ch.DirPath = SettingsViewModel.DirPath;

            var lst = new List<IVideoItem>();
            channel.ChannelItems.ForEach(lst.Add);

            foreach (IVideoItem item in lst)
            {
                ch.AddNewItem(item);
            }
            SetStatus(1);

            if (ch.ChannelItems.Count == YouTubeSite.ItemsPerPage)
            {
                IEnumerable<VideoItemPOCO> allitem = await YouTubeSite.GetChannelItemsAsync(channel.ID, 0, true);
                foreach (IVideoItem item in allitem.Select(VideoItemFactory.CreateVideoItem))
                {
                    ch.AddNewItem(item);
                }
            }

            List<PlaylistPOCO> pls = await YouTubeSite.GetChannelPlaylistsNetAsync(channel.ID);
            foreach (PlaylistPOCO poco in pls)
            {
                poco.PlaylistItems = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(poco.ID, 0);
                ch.ChannelPlaylists.Add(PlaylistFactory.CreatePlaylist(poco));
            }

            await df.InsertChannelFullAsync(channel);
            channel.ChannelState = ChannelState.Added;
            SetStatus(0);
        }

        private async void SyncChannel(object obj)
        {
            var channel = obj as IChannel;
            if (channel == null)
            {
                return;
            }

            SetStatus(1);
            Info = "Syncing: " + channel.Title;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                await ChannelFactory.SyncChannelAsync(channel, false, true);
                watch.Stop();
                Info = watch.TakeLogMessage();
                SetStatus(0);
            }
            catch (Exception ex)
            {
                SetStatus(3);
                MessageBox.Show(ex.Message);
            }
        }

        private async Task SyncChannelPlaylist()
        {
            IChannel channel = SelectedChannel;
            if (channel == null || channel.ChannelState == ChannelState.InWork)
            {
                return;
            }

            SetStatus(1);
            try
            {
                await ChannelFactory.SyncChannelPlaylistsAsync(channel);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                SetStatus(3);
            }

            SetStatus(0);
        }

        private async Task SyncData(bool isFastSync)
        {
            PrValue = 0;
            int i = 0;
            SetStatus(1);
            TaskbarManager prog = TaskbarManager.Instance;
            prog.SetProgressState(TaskbarProgressBarState.Normal);
            Stopwatch watch = Stopwatch.StartNew();
            foreach (IChannel channel in Channels)
            {
                i += 1;
                PrValue = Math.Round((double)(100 * i) / Channels.Count);
                prog.SetProgressValue((int)PrValue, 100);
                Info = "Syncing: " + channel.Title;
                try
                {
                    await ChannelFactory.SyncChannelAsync(channel, isFastSync);
                }
                catch (Exception ex)
                {
                    SetStatus(3);
                    Info = ex.Message;
                }
            }

            prog.SetProgressState(TaskbarProgressBarState.NoProgress);
            PrValue = 0;
            SetStatus(0);

            Info = string.Format("Total: {0}. New: {1}. {2}", i, Channels.Sum(x => x.CountNew), watch.TakeLogMessage());
        }

        private void TagCheck()
        {
            channelCollectionView.Filter = FilterByCheckedTag;
        }

        private void UpdateChannelsDir(string obj)
        {
            if (!string.IsNullOrEmpty(obj))
            {
                foreach (IChannel channel in Channels)
                {
                    channel.DirPath = obj;
                }
                ServiceChannel.DirPath = obj;
            }
            else
            {
                SetStatus(4);
            }
        }

        private async Task Vacuumdb()
        {
            long sizebefore = df.FileBase.Length;
            await df.VacuumAsync();
            long sizeafter = new FileInfo(df.FileBase.FullName).Length;
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

                case VideoMenuItem.DeleteDb:
                    await DeleteItems(true);
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
                    SubscribeOnPopular();
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

        private void GetPocoChannelsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                var fbres = e.Result as List<ChannelPOCO>;
                if (fbres != null)
                {
                    foreach (IChannel channel in fbres.Select(poco => ChannelFactory.CreateChannel(poco, SettingsViewModel.DirPath)))
                    {
                        Channels.Add(channel);
                    }
                }
            }
            if (Channels.Any())
            {
                SelectedChannel = Channels.First();
            }

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

        private async void GetPocoChannelsDoWork(object sender, DoWorkEventArgs e)
        {
            List<ChannelPOCO> fbres = await df.GetChannelsListAsync();
            e.Result = fbres;
        }

        #endregion
    }
}
