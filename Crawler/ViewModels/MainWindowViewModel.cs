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
using Models.BO.Playlists;
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
        private const string defPlIcon = "Crawler.Images.pop.png";
        private const string txtfilter = "Text documents (.txt)|*.txt";

        #endregion

        #region Static and Readonly Fields

        private readonly Action<IVideoItem, object> addItemToStateChannel;
        private readonly ICollectionView channelCollectionView;
        private readonly SqLiteDatabase db;
        private readonly Dictionary<string, string> launchParam = new Dictionary<string, string>();

        #endregion

        #region Fields

        private RelayCommand addNewItemCommand;
        private RelayCommand changeWatchedCommand;
        private RelayCommand channelDoubleClickCommand;
        private RelayCommand channelGridFocusCommand;
        private RelayCommand channelKeyDownCommand;
        private RelayCommand channelMenuCommand;
        private RelayCommand channelSelectCommand;
        private RelayCommand clearFilterCommand;
        private RelayCommand clearChannelFilterCommand;
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
        private string selectedVideoTag;
        private RelayCommand siteChangedCommand;
        private RelayCommand syncDataCommand;
        private RelayCommand tagsDropDownOpenedCommand;
        private RelayCommand videoClickCommand;
        private ICollectionView videoCollectionView;
        private RelayCommand videoDoubleClickCommand;
        private RelayCommand videoDropDownOpenedCommand;
        private RelayCommand videoItemMenuCommand;

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
            ParseCommandLineArguments();
            db = CommonFactory.CreateSqLiteDatabase();
            if (launchParam.Any())
            {
                // через параметры запуска указали путь к своей базе
                string dbpath;
                if (launchParam.TryGetValue(dbLaunchParam, out dbpath))
                {
                    db.FileBase = new FileInfo(dbpath);
                }
            }

            SettingsViewModel = new SettingsViewModel(db, UpdateChannelsDir);
            Version = CommonExtensions.GetFileVersion(Assembly.GetExecutingAssembly());
            Channels = new ObservableCollection<IChannel>();
            channelCollectionView = CollectionViewSource.GetDefaultView(Channels);
            ServiceChannels = new ObservableCollection<ServiceChannelViewModel>();
            RelatedChannels = new ObservableCollection<IChannel>();
            CurrentTags = new ObservableCollection<ITag>();
            ServiceChannel = new ServiceChannelViewModel();
            StateChannel = new StateChannel(db);
            RelatedChannels.Add(StateChannel);
            InitBase();
            addItemToStateChannel = AddItemToStateChannel;
        }

        #endregion

        #region Properties

        public RelayCommand AddNewItemCommand => addNewItemCommand ?? (addNewItemCommand = new RelayCommand(x => AddNewItem()));

        public RelayCommand ChangeWatchedCommand => changeWatchedCommand ?? (changeWatchedCommand = new RelayCommand(ChangeWatchState));

        public RelayCommand ChannelDoubleClickCommand
            => channelDoubleClickCommand ?? (channelDoubleClickCommand = new RelayCommand(SyncChannel));

        public RelayCommand ChannelGridFocusCommand => channelGridFocusCommand ?? (channelGridFocusCommand = new RelayCommand(FocusRow));

        public RelayCommand ChannelKeyDownCommand => channelKeyDownCommand ?? (channelKeyDownCommand = new RelayCommand(ChannelKeyDown));

        public RelayCommand ChannelMenuCommand => channelMenuCommand ?? (channelMenuCommand = new RelayCommand(ChannelMenuClick));

        public ObservableCollection<IChannel> Channels { get; }

        public RelayCommand ChannelSelectCommand => channelSelectCommand ?? (channelSelectCommand = new RelayCommand(ScrollToTop));

        public RelayCommand ClearFilterCommand
            => clearFilterCommand ?? (clearFilterCommand = new RelayCommand(x => SelectedChannel.FilterVideoKey = string.Empty));

        public RelayCommand ClearChannelFilterCommand
            => clearChannelFilterCommand ?? (clearChannelFilterCommand = new RelayCommand(x => FilterChannelKey = string.Empty));

        public RelayCommand CurrentTagCheckedCommand
            => currentTagCheckedCommand ?? (currentTagCheckedCommand = new RelayCommand(x => TagCheck()));

        public ObservableCollection<ITag> CurrentTags { get; }

        public RelayCommand FillChannelsCommand => fillChannelsCommand ?? (fillChannelsCommand = new RelayCommand(OnStartup));

        public RelayCommand FillDescriptionCommand
            => fillDescriptionCommand ?? (fillDescriptionCommand = new RelayCommand(FillDescription));

        public RelayCommand FillRelatedChannelCommand
            => fillRelatedChannelCommand ?? (fillRelatedChannelCommand = new RelayCommand(x => FillRelated()));

        public RelayCommand FillSubitlesCommand => fillSubitlesCommand ?? (fillSubitlesCommand = new RelayCommand(FillSubtitles));

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
                if (value == info)
                {
                    return;
                }
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
                if (value == isWorking)
                {
                    return;
                }
                isWorking = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand MainMenuCommand => mainMenuCommand ?? (mainMenuCommand = new RelayCommand(MainMenuClick));

        public RelayCommand OpenDescriptionCommand
            => openDescriptionCommand ?? (openDescriptionCommand = new RelayCommand(OpenDescription));

        public RelayCommand PlaylistExpandCommand => playlistExpandCommand ?? (playlistExpandCommand = new RelayCommand(PlaylistExpand));

        public RelayCommand PlaylistMenuCommand => playlistMenuCommand ?? (playlistMenuCommand = new RelayCommand(PlaylistMenuClick));

        public RelayCommand PlaylistSelectCommand => playlistSelectCommand ?? (playlistSelectCommand = new RelayCommand(SelectPlaylist));

        public RelayCommand PopularFillCommand => popularFillCommand ?? (popularFillCommand = new RelayCommand(x => FillPopular()));

        public RelayCommand PopularSelectCommand => popularSelectCommand ?? (popularSelectCommand = new RelayCommand(SelectPopular));

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

        public ObservableCollection<IChannel> RelatedChannels { get; }

        public string Result
        {
            get
            {
                return result;
            }
            private set
            {
                if (value == result)
                {
                    return;
                }
                result = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand ScrollChangedCommand => scrollChangedCommand ?? (scrollChangedCommand = new RelayCommand(ScrollChanged));

        public RelayCommand SearchCommand
            => searchCommand ?? (searchCommand = new RelayCommand(async x => await SearchExecute().ConfigureAwait(false)));

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
                videoCollectionView = CollectionViewSource.GetDefaultView(selectedChannel.ChannelItems);
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

        public string SelectedVideoTag
        {
            get
            {
                return selectedVideoTag;
            }
            set
            {
                if (Equals(value, selectedVideoTag))
                {
                    return;
                }
                selectedVideoTag = value;
                videoCollectionView.Filter = FilterVideosByTag;
                OnPropertyChanged();
            }
        }

        public ServiceChannelViewModel ServiceChannel { get; set; }
        public ObservableCollection<ServiceChannelViewModel> ServiceChannels { get; set; }

        public SettingsViewModel SettingsViewModel { get; }

        public RelayCommand SiteChangedCommand => siteChangedCommand ?? (siteChangedCommand = new RelayCommand(SiteChanged));

        public StateChannel StateChannel { get; }

        public RelayCommand SyncDataCommand
            => syncDataCommand ?? (syncDataCommand = new RelayCommand(async x => await SyncData(true).ConfigureAwait(false)));

        public RelayCommand TagsDropDownOpenedCommand
            => tagsDropDownOpenedCommand ?? (tagsDropDownOpenedCommand = new RelayCommand(x => OpenCurrentTags()));

        public string Version { get; private set; }

        public RelayCommand VideoClickCommand => videoClickCommand ?? (videoClickCommand = new RelayCommand(RunItemOneClick));

        public RelayCommand VideoDoubleClickCommand
            => videoDoubleClickCommand ?? (videoDoubleClickCommand = new RelayCommand(RunItemDoubleClick));

        public RelayCommand VideoItemMenuCommand => videoItemMenuCommand ?? (videoItemMenuCommand = new RelayCommand(VideoItemMenuClick));

        public RelayCommand VideoTagsDropDownOpenedCommand
            => videoDropDownOpenedCommand ?? (videoDropDownOpenedCommand = new RelayCommand(x => FillVideoTags()));

        #endregion

        #region Static Methods

        private static void AddDefPlaylist(IChannel channel, List<string> ids)
        {
            Stream img = Assembly.GetExecutingAssembly().GetManifestResourceStream(defPlIcon);
            IPlaylist defpl = PlaylistFactory.CreateUploadPlaylist(channel, ids, StreamHelper.ReadFully(img));
            channel.ChannelPlaylists.Add(defpl);
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

        private static void ScrollToTop(object obj)
        {
            var vGrid = obj as DataGrid;
            if (vGrid == null)
            {
                return;
            }
            ScrollViewer scr = UiExtensions.GetScrollbar(vGrid);
            scr?.ScrollToTop();
        }

        #endregion

        #region Methods

        private void AddItemToDownloadToList(IVideoItem item)
        {
            ServiceChannel.AddItemToDownload(item);
            SelectedChannel = ServiceChannel;
        }

        private void AddItemToStateChannel(IVideoItem videoItem, object state)
        {
            StateChannel.AddToStateList(state, videoItem);
        }

        private async void AddNewChannel(string channelId, string channelTitle, SiteType site)
        {
            string parsedId = null;

            switch (site)
            {
                case SiteType.YouTube:
                    parsedId = await YouTubeSite.ParseChannelLink(channelId).ConfigureAwait(false);

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
                        await AddNewChannelAsync(parsedId, channelTitle, site, true).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private async Task AddNewChannelAsync(string channelid, string channeltitle, SiteType site, bool useFast)
        {
            SetStatus(1);

            IChannel channel = await Task.Run(() => ChannelFactory.GetChannelNetAsync(channelid, site)).ConfigureAwait(false);

            if (channel == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(channeltitle))
            {
                channel.Title = channeltitle;
            }

            if (Application.Current.CheckAccess())
            {
                Channels.Add(channel);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => Channels.Add(channel));
            }
            channel.DirPath = SettingsViewModel.DirPath;
            channel.ChannelState = ChannelState.InWork;
            channel.ChannelItemsCount = channel.ChannelItems.Count;
            channel.PlaylistCount = channel.ChannelPlaylists.Count;
            channel.ChannelState = ChannelState.Added;
            channel.UseFast = useFast;
            await Task.Run(() => db.InsertChannelFullAsync(channel)).ConfigureAwait(false);
            AddDefPlaylist(channel, channel.ChannelItems.Select(x => x.ID).ToList());
            SetStatus(0);
            SelectedChannel = channel;
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

        private async Task AddTags()
        {
            Dictionary<TagPOCO, IEnumerable<string>> tagsch = await db.GetChannelIdsByTagAsync().ConfigureAwait(false);
            foreach (KeyValuePair<TagPOCO, IEnumerable<string>> pair in tagsch)
            {
                ITag tag = TagFactory.CreateTag(pair.Key);
                CurrentTags.Add(tag);
                foreach (IChannel ch in pair.Value.Select(id => Channels.FirstOrDefault(x => x.ID == id)).Where(ch => ch != null))
                {
                    ch.ChannelTags.Add(tag);
                }
            }
            CurrentTags.Add(TagFactory.CreateTag()); // empty tag
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
                List<ChannelPOCO> lst = await db.GetChannelsListAsync().ConfigureAwait(false);
                var sb = new StringBuilder();
                foreach (ChannelPOCO poco in lst)
                {
                    sb.Append(poco.Title).Append("|").Append(poco.ID).Append("|").Append(poco.Site).Append(Environment.NewLine);
                }
                try
                {
                    File.WriteAllText(dlg.FileName, sb.ToString().TrimEnd('\r', '\n'));
                    Info = $"{lst.Count} channels has been stored";
                    SetStatus(0);
                }
                catch (Exception ex)
                {
                    Info = ex.Message;
                    SetStatus(3);
                }
            }
        }

        private async void ChangeWatchState(object obj)
        {
            var item = obj as IVideoItem;
            if (item == null)
            {
                return;
            }

            if (SelectedChannel is ServiceChannelViewModel && item.SyncState != SyncState.Added)
            {
                return;
            }

            switch (item.WatchState)
            {
                case WatchState.Notset:
                    item.WatchState = WatchState.Watched;
                    break;
                case WatchState.Watched:
                    item.WatchState = WatchState.Planned;
                    break;
                case WatchState.Planned:
                    item.WatchState = WatchState.Notset;
                    break;
            }
            StateChannel.AddToStateList(item.WatchState, item);
            await db.UpdateItemWatchState(item.ID, item.WatchState).ConfigureAwait(false);
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
                    List<string> ids = DeleteChannels();
                    if (ids.Any())
                    {
                        await Task.Run(() => db.DeleteChannelsAsync(ids)).ConfigureAwait(false);
                    }
                    break;

                case KeyboardKey.Enter:
                    await SearchExecute().ConfigureAwait(true);
                    break;
            }
        }

        private async void ChannelMenuClick(object param)
        {
            var menu = (ChannelMenuItem)param;
            switch (menu)
            {
                case ChannelMenuItem.Delete:

                    List<string> ids = DeleteChannels();
                    if (ids.Any())
                    {
                        await Task.Run(() => db.DeleteChannelsAsync(ids)).ConfigureAwait(false);
                    }

                    break;

                case ChannelMenuItem.Edit:
                    EditChannel();
                    break;

                case ChannelMenuItem.Related:
                    await FindRelatedChannels(SelectedChannel).ConfigureAwait(false);
                    break;

                case ChannelMenuItem.Subscribe:
                    try
                    {
                        await SubscribeOnRelated().ConfigureAwait(false);
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
                    await SyncChannelPlaylist().ConfigureAwait(false);
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

        private void CopyToClipboard(VideoMenuItem menu)
        {
            try
            {
                string res;
                switch (menu)
                {
                    case VideoMenuItem.Link:
                        res = SelectedChannel.SelectedItem.MakeLink();
                        break;
                    case VideoMenuItem.Title:
                        res = SelectedChannel.SelectedItem.Title;
                        break;
                    default:
                        res = string.Empty;
                        break;
                }
                if (!string.IsNullOrEmpty(res))
                {
                    Clipboard.SetText(res);
                }
            }
            catch (Exception ex)
            {
                Info = ex.Message;
            }
        }

        private List<string> DeleteChannels()
        {
            var res = new List<string>();
            var sb = new StringBuilder();

            foreach (IChannel channel in SelectedChannels)
            {
                sb.Append(channel.Title).Append(Environment.NewLine);
            }

            if (sb.Length == 0)
            {
                return res;
            }

            MessageBoxResult boxResult = MessageBox.Show($"Delete:{Environment.NewLine}{sb}?",
                "Confirm",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information);

            if (boxResult != MessageBoxResult.OK)
            {
                return res;
            }
            var indexes = new List<int>();
            List<IChannel> channels = SelectedChannels.OfType<IChannel>().ToList();
            for (int i = channels.Count; i > 0; i--)
            {
                IChannel channel = channels.ElementAt(i - 1);
                indexes.Add(Channels.IndexOf(channel));
                Channels.Remove(channel);
                res.Add(channel.ID);
                channelCollectionView.Filter = null;
                FilterChannelKey = string.Empty;
                foreach (IVideoItem elem in
                    channel.ChannelItems.Select(item => StateChannel.ChannelItems.FirstOrDefault(x => x.ID == item.ID))
                        .Where(elem => elem != null))
                {
                    StateChannel.ChannelItems.Remove(elem);
                    StateChannel.ChannelItemsCount--;
                }
            }

            if (!Channels.Any() || !indexes.Any())
            {
                return res;
            }
            int pos = indexes.Count == 1 ? indexes[0] : indexes.Min();

            if (pos == Channels.Count)
            {
                SelectedChannel = Channels.Last();
            }
            else if (pos == 0)
            {
                SelectedChannel = Channels.First();
            }
            else
            {
                SelectedChannel = Channels[pos - 1];
            }
            return res;
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
                            await db.DeleteItemAsync(item.ID).ConfigureAwait(false);
                            if (item.SyncState == SyncState.Added)
                            {
                                StateChannel.AddToStateList(SyncState.Notset, item);
                            }
                            else if (item.WatchState == WatchState.Watched || item.WatchState == WatchState.Planned)
                            {
                                StateChannel.AddToStateList(item.WatchState, item);
                            }
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
                    await db.UpdateChannelNewCountAsync(channel.ID, channel.CountNew).ConfigureAwait(false);
                }
            }
        }

        private async Task DownloadWithOptions(VideoMenuItem item)
        {
            if (!SettingsViewModel.IsYoutubeExist())
            {
                return;
            }

            IChannel channel = SelectedChannel;
            if (channel == null)
            {
                return;
            }

            switch (item)
            {
                case VideoMenuItem.Video:

                    channel.ChannelState = ChannelState.InWork;
                    await
                        channel.SelectedItem.DownloadItem(SettingsViewModel.YouPath, SettingsViewModel.DirPath, PlaylistMenuItem.Video)
                            .ConfigureAwait(false);
                    channel.ChannelState = ChannelState.HasDownload;

                    break;

                case VideoMenuItem.Audio:

                    channel.ChannelState = ChannelState.InWork;
                    await
                        channel.SelectedItem.DownloadItem(SettingsViewModel.YouPath, SettingsViewModel.DirPath, PlaylistMenuItem.Audio)
                            .ConfigureAwait(false);
                    channel.ChannelState = ChannelState.HasDownload;

                    break;

                case VideoMenuItem.Subtitles:
                    channel.ChannelState = ChannelState.InWork;
                    await
                        channel.SelectedItem.DownloadItem(SettingsViewModel.YouPath,
                            SettingsViewModel.DirPath,
                            PlaylistMenuItem.DownloadSubsOnly).ConfigureAwait(false);
                    break;

                case VideoMenuItem.HD:

                    if (SettingsViewModel.IsFfmegExist())
                    {
                        channel.ChannelState = ChannelState.InWork;
                        await
                            channel.SelectedItem.DownloadItem(SettingsViewModel.YouPath,
                                SettingsViewModel.DirPath,
                                PlaylistMenuItem.DownloadHd).ConfigureAwait(false);
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

        private void FillChannelItems(IChannel channel)
        {
            IsLogExpand = false;
            IsPlExpand = false;

            if (channel == null)
            {
                return;
            }

            channel.IsShowSynced = false;

            if (channel is StateChannel)
            {
                return;
            }

            // если канал только что добавили - элементы там есть
            // есть новые элементы после синхронизации - теперь проставляется в самой синхронизации
            // если канал заполнен элементами, но нет новых - уже загружали, не нужно больше
            if (channel.Loaded || channel.ChannelState == ChannelState.Added)
            {
                channel.RefreshView("Timestamp");
                return;
            }

            // заполняем только если либо ничего нет, либо одни новые (после сунка или после того, как была выборка по стейту)
            if (channel.IsHasNewFromSync)
            {
                List<string> excepted = channel.ChannelItems.Select(x => x.ID).ToList();
                ChannelFactory.FillChannelItemsFromDbAsync(channel, basePage, excepted);
            }
            else
            {
                ChannelFactory.FillChannelItemsFromDbAsync(channel, basePage);
            }

            channel.Loaded = true;
            channel.IsHasNewFromSync = false;
            ChannelFactory.SetChannelCountAsync(channel);
        }

        private async void FillDescription(object obj)
        {
            var video = obj as IVideoItem;
            if (video != null)
            {
                if (string.IsNullOrEmpty(video.Description))
                {
                    await video.FillDescriptionAsync().ConfigureAwait(false);
                }
            }
            else
            {
                var channel = obj as IChannel;
                if (channel == null)
                {
                    return;
                }
                if (!string.IsNullOrEmpty(channel.SubTitle))
                {
                    return;
                }
                string text = await db.GetChannelDescriptionAsync(channel.ID).ConfigureAwait(false);
                channel.SubTitle = text.WordWrap(80);
            }
        }

        private async void FillPopular()
        {
            SetStatus(1);
            Dictionary<string, string> dic = Channels.ToDictionary(x => x.ID, y => y.Title);
            await ServiceChannel.FillPopular(dic).ConfigureAwait(true);
            SelectedChannel.ChannelItemsCount = ServiceChannel.ChannelItems.Count;
            SetStatus(0);
        }

        private async void FillRelated()
        {
            var channel = SelectedChannel as YouChannel;
            if (channel == null || channel.ChannelItems.Any())
            {
                return;
            }

            SetStatus(1);
            channel.ChannelState = ChannelState.InWork;
            IEnumerable<IVideoItem> lst = await ChannelFactory.GetChannelItemsNetAsync(channel, 0).ConfigureAwait(true);
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
                await item.FillSubtitles().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Info = ex.Message;
            }
        }

        private async void FillVideoTags()
        {
            SelectedChannel.VideoTags.Clear();
            IEnumerable<string> parents = SelectedChannel.ChannelItems.Select(x => x.ParentID).Distinct();
            foreach (string parent in parents)
            {
                List<TagPOCO> tags = await db.GetChannelTagsAsync(parent).ConfigureAwait(false);
                foreach (TagPOCO tag in
                    tags.Where(tag => !SelectedChannel.VideoTags.Contains(tag.Title)))
                {
                    SelectedChannel.VideoTags.Add(tag.Title);
                }
                foreach (IVideoItem item in SelectedChannel.ChannelItems.Where(item => item.ParentID == parent))
                {
                    item.Tags = tags.Select(x => x.Title);
                }
            }
            if (SelectedChannel.VideoTags.Any())
            {
                SelectedChannel.VideoTags.Add(string.Empty);
            }
        }

        private bool FilterByCheckedTag(object item)
        {
            var channel = (IChannel)item;
            if (channel?.ChannelTags == null)
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
            if (value?.Title == null)
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
            if (channel?.ChannelTags == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(SelectedTag?.Title))
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

        private bool FilterVideosByTag(object obj)
        {
            var item = (IVideoItem)obj;
            return string.IsNullOrEmpty(SelectedVideoTag) || item.Tags.Contains(SelectedVideoTag);
        }

        private async Task FindRelatedChannels(IChannel channel)
        {
            if (channel == null)
            {
                return;
            }

            SetStatus(1);

            for (int i = RelatedChannels.Count; i > 0; i--)
            {
                IChannel ch = RelatedChannels[i - 1];
                if (!(ch is StateChannel))
                {
                    RelatedChannels.Remove(ch);
                }
            }

            IEnumerable<IChannel> lst = await ChannelFactory.GetRelatedChannelNetAsync(channel).ConfigureAwait(true);

            var ids = new HashSet<string>(Channels.Select(x => x.ID));
            foreach (IChannel ch in lst)
            {
                if (ids.Contains(ch.ID))
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
                    await SettingsViewModel.LoadSettingsFromLaunchParam(launchParam).ConfigureAwait(false);
                }
                else
                {
                    await SettingsViewModel.LoadSettingsFromDb().ConfigureAwait(false);
                }

                await SettingsViewModel.LoadTagsFromDb().ConfigureAwait(false);
                await SettingsViewModel.LoadCredsFromDb().ConfigureAwait(false);
                ServiceChannel.Init(SettingsViewModel.SupportedCreds, SettingsViewModel.DirPath, db);
                ServiceChannels.Add(ServiceChannel);
                StateChannel.DirPath = SettingsViewModel.DirPath;
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
                        await Backup().ConfigureAwait(false);
                        break;

                    case MainMenuItem.Restore:
                        await Restore().ConfigureAwait(false);
                        break;

                    case MainMenuItem.Settings:
                        OpenSettings();
                        break;

                    case MainMenuItem.Sync:
                        await SyncData(false).ConfigureAwait(false);
                        break;

                    case MainMenuItem.Vacuum:
                        await Vacuumdb().ConfigureAwait(false);
                        break;

                    case MainMenuItem.Link:
                        OpenAddLink();
                        break;

                    case MainMenuItem.About:

                        var aboutVm = new AboutViewModel { Result = await db.GetWatchedStatistics() };
                        var abview = new AboutView
                        {
                            DataContext = aboutVm,
                            Owner = Application.Current.MainWindow,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };
                        abview.ShowDialog();

                        break;
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void OnStartup(object obj)
        {
            var channelGrid = obj as DataGrid;
            try
            {
                Result = "Working..";
                List<ChannelPOCO> fbres = await Task.Run(() => db.GetChannelsListAsync()).ConfigureAwait(true);
                foreach (IChannel channel in fbres.Select(poco => ChannelFactory.CreateChannel(poco, SettingsViewModel.DirPath)))
                {
                    Channels.Add(channel);
                }
                if (SettingsViewModel.IsFilterOpen)
                {
                    StateChannel.Init(Channels);
                    SelectedChannel = StateChannel;
                }
                else
                {
                    if (Channels.Any())
                    {
                        SelectedChannel = Channels.First();
                    }
                }

                SetStatus(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            // focus
            if (channelGrid == null || channelGrid.SelectedIndex < 0)
            {
                return;
            }
            channelGrid.UpdateLayout();
            var selectedRow = (DataGridRow)channelGrid.ItemContainerGenerator.ContainerFromIndex(channelGrid.SelectedIndex);
            selectedRow?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
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
            CurrentTags.Clear();
            await AddTags().ConfigureAwait(false);
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

        private async void OpenTags()
        {
            IChannel channel = SelectedChannel;
            if (channel == null)
            {
                return;
            }

            if (channel.ChannelTags.Any())
            {
                channel.ChannelTags.Clear();
            }

            List<TagPOCO> tags = await db.GetChannelTagsAsync(channel.ID).ConfigureAwait(false);
            tags.ForEach(x => channel.ChannelTags.Add(TagFactory.CreateTag(x)));

            var etvm = new EditTagsViewModel(channel, SettingsViewModel.SupportedTags, db, ChannelTagDelete);
            var etv = new EditTagsView
            {
                DataContext = etvm,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = $"Tags: {channel.Title}"
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

        private async void PlaylistExpand(object obj)
        {
            var channel = obj as YouChannel;
            if (channel == null)
            {
                if (!SettingsViewModel.IsFilterOpen)
                {
                    StateChannel.Init(Channels);
                }
                SelectedChannel = StateChannel;
                return;
            }
            if (channel.ChannelPlaylists.Any() && channel.ChannelPlaylists.Any(x => x.State != SyncState.Added))
            {
                return;
            }

            List<PlaylistPOCO> fbres = await db.GetChannelPlaylistAsync(channel.ID).ConfigureAwait(false);

            foreach (IPlaylist pl in fbres.Select(poco => PlaylistFactory.CreatePlaylist(poco, channel.Site)))
            {
                channel.ChannelPlaylists.Add(pl);
            }

            List<string> lst = await db.GetChannelItemsIdListDbAsync(channel.ID, 0, 0).ConfigureAwait(false);
            AddDefPlaylist(channel, lst);
        }

        private void PlaylistLinkToClipboard()
        {
            try
            {
                string link = string.Empty;
                switch (SelectedPlaylist.Site)
                {
                    case SiteType.YouTube:
                        link = $"https://www.youtube.com/playlist?list={SelectedPlaylist.ID}";
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
                case PlaylistMenuItem.DownloadHd:
                case PlaylistMenuItem.DownloadSubs:
                case PlaylistMenuItem.Audio:
                case PlaylistMenuItem.Video:
                    try
                    {
                        await
                            PlaylistFactory.DownloadPlaylist(playlist, SelectedChannel, SettingsViewModel.YouPath, menu)
                                .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Info = ex.Message;
                    }

                    break;

                case PlaylistMenuItem.Update:

                    if (SelectedChannel == null || SelectedChannel is ServiceChannelViewModel)
                    {
                        return;
                    }

                    SetStatus(1);
                    try
                    {
                        await PlaylistFactory.UpdatePlaylist(playlist, SelectedChannel).ConfigureAwait(false);
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
                var rest = 0;
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
                                        await AddNewChannelAsync(sp[1], sp[0], backupSiteType, true).ConfigureAwait(false);
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
                    PrValue = Math.Round((double)(100 * rest) / lst.Length);
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
                var chanell = SelectedChannel as YouChannel;
                chanell?.RestoreFullChannelItems();
            }
            else if (SettingsViewModel.IsMpcExist())
            {
                await item.RunItem(SettingsViewModel.MpcPath).ConfigureAwait(false);
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
                    await item.RunItem(SettingsViewModel.MpcPath).ConfigureAwait(false);
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
                await item.DownloadItem(fn.FullName, SettingsViewModel.DirPath, PlaylistMenuItem.Download).ConfigureAwait(false);
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
            channel?.RestoreFullChannelItems();
        }

        private async Task SearchExecute()
        {
            SetStatus(1);
            Dictionary<string, string> dic = Channels.ToDictionary(x => x.ID, y => y.Title);
            await ServiceChannel.Search(dic).ConfigureAwait(true);
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

            if (!(obj is YouPlaylist) || !(SelectedChannel is YouChannel))
            {
                return;
            }
            var channel = (YouChannel)SelectedChannel;
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
            var channel = obj as IChannel;
            if (channel != null)
            {
                SelectedChannel = channel;
            }
            else
            {
                var image = obj as StateChannel.StateImage;
                if (image == null)
                {
                    return;
                }
                StateChannel.SelectedState = image;
                SelectedChannel = StateChannel;
            }
        }

        private async void SubscribeOnPopular()
        {
            IChannel channel = SelectedChannel;
            if (channel?.SelectedItem == null || Channels.Select(x => x.ID).Contains(channel.SelectedItem.ParentID))
            {
                return;
            }
            IVideoItem item = channel.SelectedItem;
            await AddNewChannelAsync(item.ParentID, string.Empty, SiteType.YouTube, true).ConfigureAwait(false);
            item.SyncState = SyncState.Added;
        }

        private async Task SubscribeOnRelated()
        {
            var channel = SelectedChannel as YouChannel;
            if (channel == null || channel.ChannelState == ChannelState.InWork || Channels.Select(x => x.ID).Contains(channel.ID))
            {
                return;
            }

            SetStatus(1);
            List<PlaylistPOCO> pls = await YouTubeSite.GetChannelPlaylistsNetAsync(channel.ID).ConfigureAwait(true);
            foreach (PlaylistPOCO poco in pls)
            {
                poco.PlaylistItems = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(poco.ID, 0).ConfigureAwait(true);
                channel.ChannelPlaylists.Add(PlaylistFactory.CreatePlaylist(poco, channel.Site));
            }
            channel.UseFast = true;
            await db.InsertChannelFullAsync(channel).ConfigureAwait(false);
            channel.ChannelState = ChannelState.Added;
            channel.PlaylistCount = channel.ChannelPlaylists.Count;
            SetStatus(0);
            Channels.Add(channel);
            RelatedChannels.Remove(RelatedChannels.First(x => x.ID == channel.ID));
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
                await ChannelFactory.SyncChannelAsync(channel, false, true, addItemToStateChannel).ConfigureAwait(true);
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
                await ChannelFactory.SyncChannelPlaylistsAsync(channel).ConfigureAwait(false);
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
            var i = 0;
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
                    await ChannelFactory.SyncChannelAsync(channel, isFastSync, false, addItemToStateChannel).ConfigureAwait(true);
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

            Info = $"Total: {i}. New: {Channels.Sum(x => x.CountNew)}. {watch.TakeLogMessage()}";
        }

        private void TagCheck()
        {
            channelCollectionView.Filter = FilterByCheckedTag;
        }

        private void UpdateChannelsDir(string obj)
        {
            if (!string.IsNullOrEmpty(obj))
            {
                Channels.ForEach(x => x.DirPath = obj);
                ServiceChannel.DirPath = obj;
                StateChannel.DirPath = obj;
            }
            else
            {
                SetStatus(4);
            }
        }

        private async Task Vacuumdb()
        {
            long sizebefore = db.FileBase.Length;
            await db.VacuumAsync().ConfigureAwait(false);
            long sizeafter = new FileInfo(db.FileBase.FullName).Length;
            Info = $"Database compacted (bytes): {sizebefore} -> {sizeafter}";
        }

        private async void VideoItemMenuClick(object param)
        {
            var menu = (VideoMenuItem)param;
            switch (menu)
            {
                case VideoMenuItem.Delete:
                    await DeleteItems().ConfigureAwait(false);
                    break;

                case VideoMenuItem.DeleteDb:
                    await DeleteItems(true).ConfigureAwait(false);
                    break;

                case VideoMenuItem.Audio:
                case VideoMenuItem.HD:
                case VideoMenuItem.Subtitles:
                    await DownloadWithOptions(menu).ConfigureAwait(false);
                    break;

                case VideoMenuItem.Link:
                    CopyToClipboard(VideoMenuItem.Link);
                    break;

                case VideoMenuItem.Parent:
                    if (SelectedChannel is StateChannel || SelectedChannel is ServiceChannelViewModel)
                    {
                        IChannel channel = Channels.FirstOrDefault(x => x.ID == SelectedChannel.SelectedItem.ParentID);
                        if (channel != null)
                        {
                            SelectedChannel = channel;
                        }
                    }
                    break;

                case VideoMenuItem.Title:
                    CopyToClipboard(VideoMenuItem.Title);
                    break;

                case VideoMenuItem.Subscribe:
                    SubscribeOnPopular();
                    break;

                case VideoMenuItem.Cancel:
                    var item = SelectedChannel.SelectedItem as YouTubeItem;
                    item?.CancelDownload();
                    break;

                case VideoMenuItem.Folder:
                    var dir = new DirectoryInfo(Path.Combine(SelectedChannel.DirPath, SelectedChannel.ID));
                    string par = dir.Exists ? dir.FullName : SelectedChannel.DirPath;
                    SelectedChannel.SelectedItem.OpenInFolder(par);
                    break;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
