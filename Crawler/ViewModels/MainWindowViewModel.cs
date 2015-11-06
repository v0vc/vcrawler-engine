// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Crawler.Common;
using Crawler.Models;
using Crawler.Views;
using Interfaces.API;
using Interfaces.Enums;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Microsoft.WindowsAPICodePack.Taskbar;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;

namespace Crawler.ViewModels
{
    public class MainWindowViewModel
    {
        #region Fields

        private RelayCommand addNewItemCommand;
        private RelayCommand addNewTagCommand;
        private RelayCommand channelDoubleClickCommand;
        private RelayCommand channelKeyDownCommand;
        private RelayCommand channelMenuCommand;
        private RelayCommand channelSelectionChangedCommand;
        private RelayCommand currentTagCheckedCommand;
        private RelayCommand currentTagSelectionChangedCommand;
        private RelayCommand downloadLinkCommand;
        private RelayCommand fillChannelsCommand;
        private RelayCommand fillDescriptionCommand;
        private RelayCommand fillPopularCommand;
        private bool isHasBeenFocused;
        private RelayCommand mainMenuCommand;
        private RelayCommand openDescriptionCommand;
        private RelayCommand openDirCommand;
        private RelayCommand playlistDoubleClickCommand;
        private RelayCommand playlistExpandCommand;
        private RelayCommand playlistMenuCommand;
        private RelayCommand playlistSelectCommand;
        private RelayCommand popularSelectCommand;
        private RelayCommand saveCommand;
        private RelayCommand saveNewItemCommand;
        private RelayCommand searchCommand;
        private RelayCommand submenuOpenedCommand;
        private RelayCommand syncDataCommand;
        private RelayCommand tagsDropDownOpenedCommand;
        private RelayCommand videoClickCommand;
        private RelayCommand videoDoubleClickCommand;
        private RelayCommand videoItemMenuCommand;
        private RelayCommand scrollChangedCommand;

        #endregion

        #region Constructors

        public MainWindowViewModel(MainWindowModel model)
        {
            Model = model;
        }

        #endregion

        #region Properties

        public RelayCommand AddNewItemCommand
        {
            get
            {
                return addNewItemCommand ?? (addNewItemCommand = new RelayCommand(x => AddNewItem(false)));
            }
        }

        public RelayCommand AddNewTagCommand
        {
            get
            {
                return addNewTagCommand ?? (addNewTagCommand = new RelayCommand(x => AddNewTag()));
            }
        }

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

        public RelayCommand CurrentTagSelectionChangedCommand
        {
            get
            {
                return currentTagSelectionChangedCommand ?? (currentTagSelectionChangedCommand = new RelayCommand(SelectTag));
            }
        }

        public RelayCommand DownloadLinkCommand
        {
            get
            {
                return downloadLinkCommand ?? (downloadLinkCommand = new RelayCommand(async x => await Model.DownloadLink()));
            }
        }

        public RelayCommand FillChannelsCommand
        {
            get
            {
                return fillChannelsCommand ?? (fillChannelsCommand = new RelayCommand(x => Model.OnStartup()));
            }
        }

        public RelayCommand FillDescriptionCommand
        {
            get
            {
                return fillDescriptionCommand ?? (fillDescriptionCommand = new RelayCommand(FillDescription));
            }
        }

        public RelayCommand FillPopularCommand
        {
            get
            {
                return fillPopularCommand ?? (fillPopularCommand = new RelayCommand(FillPopular));
            }
        }

        public bool IsSearchExpanded { get; set; }

        public RelayCommand MainMenuCommand
        {
            get
            {
                return mainMenuCommand ?? (mainMenuCommand = new RelayCommand(MainMenuClick));
            }
        }

        public MainWindowModel Model { get; set; }

        public RelayCommand OpenDescriptionCommand
        {
            get
            {
                return openDescriptionCommand ?? (openDescriptionCommand = new RelayCommand(OpenDescription));
            }
        }

        public RelayCommand OpenDirCommand
        {
            get
            {
                return openDirCommand ?? (openDirCommand = new RelayCommand(OpenDir));
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

        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ?? (saveCommand = new RelayCommand(async x => await Model.SaveSettings()));
            }
        }

        public RelayCommand SaveNewItemCommand
        {
            get
            {
                return saveNewItemCommand ?? (saveNewItemCommand = new RelayCommand(async x => await Model.SaveNewItem()));
            }
        }

        public RelayCommand SearchCommand
        {
            get
            {
                return searchCommand ?? (searchCommand = new RelayCommand(async x => await Model.Search()));
            }
        }

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
                return syncDataCommand ?? (syncDataCommand = new RelayCommand(async x => await Model.SyncData()));
            }
        }

        public RelayCommand TagsDropDownOpenedCommand
        {
            get
            {
                return tagsDropDownOpenedCommand ?? (tagsDropDownOpenedCommand = new RelayCommand(x => OpenCurrentTags()));
            }
        }

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

        public RelayCommand ScrollChangedCommand
        {
            get
            {
                return scrollChangedCommand ?? (scrollChangedCommand = new RelayCommand(ScrollChanged));
            }
        }

        private async void ScrollChanged(object obj)
        {
            var scroll = obj as ScrollViewer;
            if (scroll == null)
            {
                return;
            }
            if (scroll.VerticalOffset <= 0)
            {
                return;
            }
            if (Model.SelectedChannel.ChannelItemsCount > Model.SelectedChannel.ChannelItems.Count)
            {
                await
                    Model.SelectedChannel.FillChannelItemsDbAsync(Model.DirPath,
                        Model.SelectedChannel.ChannelItemsCount - Model.SelectedChannel.ChannelItems.Count,
                        Model.SelectedChannel.ChannelItems.Count);
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
            var edv = new EditDescriptionView
            {
                DataContext = item, 
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

        private void AddNewItem(bool isEditMode)
        {
            Model.IsEditMode = isEditMode;

            var addview = new AddChanelView
            {
                DataContext = this, 
                Owner = Application.Current.MainWindow, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            addview.ShowDialog();
        }

        private void AddNewTag()
        {
            var antv = new AddNewTagView
            {
                Owner = Application.Current.MainWindow, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner, 
                DataContext = this
            };

            antv.ShowDialog();
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
                ISqLiteDatabase fb = Model.BaseFactory.CreateSqLiteDatabase();
                List<IChannelPOCO> lst = (await fb.GetChannelsListAsync()).ToList();
                var sb = new StringBuilder();
                foreach (IChannelPOCO poco in lst)
                {
                    sb.Append(poco.Title).Append("|").Append(poco.ID).Append("|").Append(poco.Site).Append(Environment.NewLine);
                }
                try
                {
                    File.WriteAllText(dlg.FileName, sb.ToString().TrimEnd('\r', '\n'));
                    Model.Info = string.Format("{0} channels has been stored", lst.Count);
                    Model.SetStatus(0);
                }
                catch (Exception ex)
                {
                    Model.Info = ex.Message;
                    Model.SetStatus(3);
                }
            }
        }

        private async void ChannelKeyDown(object par)
        {
            var key = (KeyboardKey)par;
            switch (key)
            {
                case KeyboardKey.Delete:
                    await ConfirmDelete();
                    break;

                case KeyboardKey.Enter:
                    await Model.Search();
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
                    AddNewItem(true);
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

            foreach (IChannel channel in Model.SelectedChannels)
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
                for (int i = Model.SelectedChannels.Count(); i > 0; i--)
                {
                    IChannel channel = Model.SelectedChannels[i - 1];
                    Model.Channels.Remove(channel);
                    await channel.DeleteChannelAsync();
                }

                if (Model.Channels.Any())
                {
                    Model.SelectedChannel = Model.Channels.First();
                }
            }
        }

        private async Task DeleteItems()
        {
            var sb = new StringBuilder();

            foreach (IVideoItem item in Model.SelectedChannel.SelectedItems)
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
                for (int i = Model.SelectedChannel.SelectedItems.Count; i > 0; i--)
                {
                    IVideoItem item = Model.SelectedChannel.SelectedItems[i - 1];

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

            Model.SelectedChannel.IsDownloading = true;
            await Model.SelectedVideoItem.DownloadItem(Model.YouPath, Model.DirPath, false, true);
        }

        private async Task DownloadHd()
        {
            if (!IsYoutubeExist())
            {
                return;
            }

            if (IsFfmegExist())
            {
                Model.SelectedChannel.IsDownloading = true;
                await Model.SelectedVideoItem.DownloadItem(Model.YouPath, Model.DirPath, true, false);
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

        private async void FillPopular(object obj)
        {
            var channel = obj as IChannel;
            if (channel == null)
            {
                return;
            }

            if (channel.ChannelItems.Any())
            {
                // чтоб не удалять список отдельных закачек, но почистить прошлые популярные
                for (int i = channel.ChannelItems.Count; i > 0; i--)
                {
                    if (
                        !(channel.ChannelItems[i - 1].State == ItemState.LocalYes
                          || channel.ChannelItems[i - 1].State == ItemState.Downloading))
                    {
                        channel.ChannelItems.RemoveAt(i - 1);
                    }
                }
            }

            try
            {
                Model.SetStatus(1);
                IEnumerable<IVideoItem> lst = await channel.GetPopularItemsNetAsync(Model.SelectedCountry, 30);
                foreach (IVideoItem item in lst)
                {
                    channel.AddNewItem(item, false);
                    item.IsHasLocalFileFound(Model.DirPath);
                    if (Model.Channels.Select(x => x.ID).Contains(item.ParentID))
                    {
                        // подсветим видео, если канал уже есть в подписке
                        item.IsNewItem = true;
                    }
                }
                Model.SetStatus(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Model.SetStatus(3);
            }
        }

        private async Task FindRelated()
        {
            try
            {
                await Model.FindRelatedChannels(Model.SelectedChannel);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Model.SetStatus(0);
            }
        }

        private async void FocusRow(object obj)
        {
            await Model.FillChannelItems();

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

        private bool IsMpcExist()
        {
            if (!string.IsNullOrEmpty(Model.MpcPath))
            {
                return true;
            }
            MessageBox.Show("Please, select MPC");
            return false;
        }

        private bool IsYoutubeExist()
        {
            if (!string.IsNullOrEmpty(Model.YouPath))
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
                        Model.ShowAllChannels();
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
            var adl = new AddLinkView
            {
                DataContext = this, 
                Owner = Application.Current.MainWindow, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            adl.ShowDialog();
        }

        private async void OpenCurrentTags()
        {
            if (Model.CurrentTags.Any())
            {
                return;
            }

            var tmptags = new List<ITag>();
            foreach (IChannel ch in Model.Channels)
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
                Model.CurrentTags.Add(tag);
            }
        }

        private void OpenDir(object obj)
        {
            switch (obj.ToString())
            {
                case "DirPath":
                    var dlg = new FolderBrowserDialog();
                    DialogResult res = dlg.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        Model.DirPath = dlg.SelectedPath;
                    }
                    break;

                case "MpcPath":
                    var dlgm = new OpenFileDialog { Filter = @"EXE files (*.exe)|*.exe" };
                    DialogResult resm = dlgm.ShowDialog();
                    if (resm == DialogResult.OK)
                    {
                        Model.MpcPath = dlgm.FileName;
                    }
                    break;

                case "YouPath":
                    var dlgy = new OpenFileDialog { Filter = @"EXE files (*.exe)|*.exe" };
                    DialogResult resy = dlgy.ShowDialog();
                    if (resy == DialogResult.OK)
                    {
                        Model.YouPath = dlgy.FileName;
                    }
                    break;
            }
        }

        private void OpenSettings()
        {
            var set = new SettingsView
            {
                DataContext = this, 
                Owner = Application.Current.MainWindow, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            set.ShowDialog();
        }

        private void OpenTags()
        {
            var etvm = new EditTagsViewModel
            {
                ParentChannel = Model.SelectedChannel, 
                CurrentTags = Model.CurrentTags, 
                Tags = Model.Tags, 
                Channels = Model.Channels
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

        private async void PlaylistDoubleClick(object obj)
        {
            var pl = obj as IPlaylist;
            if (pl == null)
            {
                return;
            }

            Model.SetStatus(1);

            IEnumerable<string> pls = await pl.GetPlaylistItemsIdsListNetAsync();

            IVideoItemFactory vf = Model.BaseFactory.CreateVideoItemFactory();

            pl.PlaylistItems.Clear();

            foreach (IVideoItem item in Model.SelectedChannel.ChannelItems)
            {
                item.IsShowRow = false;
            }

            foreach (string id in pls.Where(id => !pl.PlaylistItems.Select(x => x.ID).Contains(id)))
            {
                IVideoItem vi = await vf.GetVideoItemNetAsync(id, pl.Site);

                Model.SelectedChannel.AddNewItem(vi, false);

                pl.PlaylistItems.Add(vi);
            }

            Model.SetStatus(0);
        }

        private void PlaylistLinkToClipboard()
        {
            try
            {
                string link = string.Format("https://www.youtube.com/playlist?list={0}", Model.SelectedPlaylist.ID);
                Clipboard.SetText(link);
            }
            catch (Exception ex)
            {
                Model.Info = ex.Message;
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
                        await Model.SelectedPlaylist.DownloadPlaylist();
                    }
                    catch (Exception ex)
                    {
                        Model.Info = ex.Message;
                    }

                    break;
            }
        }

        private async Task Restore()
        {
            Model.Info = string.Empty;

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
                    Model.Info = ex.Message;
                    Model.SetStatus(3);
                }

                Model.SetStatus(1);
                TaskbarManager prog = TaskbarManager.Instance;
                prog.SetProgressState(TaskbarProgressBarState.Normal);
                Model.ShowAllChannels();
                var rest = 0;
                foreach (string s in lst)
                {
                    string[] sp = s.Split('|');
                    if (sp.Length == 3)
                    {
                        if (Model.Channels.Select(x => x.ID).Contains(sp[1]))
                        {
                            continue;
                        }

                        switch (sp[2])
                        {
                            case "youtube.com":

                                try
                                {
                                    Model.SetStatus(1);
                                    Model.Info = "Restoring: " + sp[0];
                                    await Model.AddNewChannelAsync(sp[1], null, SiteType.YouTube);
                                }
                                catch (Exception ex)
                                {
                                    Model.SetStatus(3);
                                    Model.Info = "Can't restore: " + sp[0];
                                    MessageBox.Show(ex.Message);
                                }

                                rest++;
                                Model.PrValue = Math.Round((double)(100 * rest) / lst.Count());
                                prog.SetProgressValue((int)Model.PrValue, 100);
                                break;

                            default:

                                Model.Info = "Unsupported site: " + sp[2];

                                break;
                        }
                    }
                    else
                    {
                        Model.Info = "Check: " + s;
                    }
                }

                prog.SetProgressState(TaskbarProgressBarState.NoProgress);
                Model.PrValue = 0;
                Model.SetStatus(0);
                Model.Info = "Total restored: " + rest;
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
                await item.RunItem(Model.MpcPath);
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
                    await item.RunItem(Model.MpcPath);
                }
            }
            else
            {
                if (!IsYoutubeExist())
                {
                    return;
                }
                var fn = new FileInfo(Model.YouPath);
                if (!fn.Exists)
                {
                    return;
                }
                Model.SelectedChannel.IsDownloading = true;
                await item.DownloadItem(fn.FullName, Model.DirPath, false, false);
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
                foreach (IVideoItem item in Model.SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = false;
                }

                foreach (IVideoItem item in Model.SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = pl.PlaylistItems.Select(x => x.ID).Contains(item.ID);
                }

                return;
            }

            pl.PlaylistItems.Clear();

            foreach (IVideoItem item in Model.SelectedChannel.ChannelItems)
            {
                item.IsShowRow = lstv.Contains(item.ID);
                if (item.IsShowRow)
                {
                    pl.PlaylistItems.Add(item);
                }
            }

            Model.Filterlist.Clear();
        }

        private void SelectPopular(object obj)
        {
            var channel = obj as IChannel;
            if (channel == null)
            {
                return;
            }
            Model.SelectedChannel = channel;
        }

        private void SelectTag(object obj)
        {
            var tag = obj as ITag;
            if (tag == null)
            {
                return;
            }

            foreach (IChannel channel in Model.Channels)
            {
                channel.IsShowRow = true;
                if (!channel.ChannelTags.Select(x => x.Title).Contains(tag.Title))
                {
                    channel.IsShowRow = false;
                }
            }

            foreach (ITag item in Model.CurrentTags)
            {
                if (item.Title != tag.Title && item.IsChecked)
                {
                    item.IsChecked = false;
                }
            }
        }

        private async Task Subscribe()
        {
            if (Model.SelectedChannel.IsInWork)
            {
                return;
            }

            Model.Channels.Add(Model.SelectedChannel);
            await Model.SelectedChannel.InsertChannelItemsAsync();
        }

        private async Task SubscribeOn()
        {
            if (Model.SelectedChannel.ID != "pop")
            {
                // этот канал по-любому есть - даже проверять не будем)
                MessageBox.Show("Has already");
                return;
            }

            await Model.AddNewChannel(Model.SelectedVideoItem.MakeLink(), Model.SelectedChannel.Site);
        }

        private async void SyncChannel(object obj)
        {
            var channel = obj as IChannel;
            if (channel != null)
            {
                await Model.SyncChannel(channel);
            }
        }

        private async Task SyncChannelPlaylist()
        {
            Model.SetStatus(1);
            if (Model.SelectedChannel.IsInWork)
            {
                return;
            }

            try
            {
                await Model.SelectedChannel.SyncChannelPlaylistsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Model.SetStatus(3);
            }

            Model.SetStatus(0);
        }

        private void TagCheck()
        {
            foreach (IChannel channel in Model.Channels)
            {
                channel.IsShowRow = false;
            }

            if (Model.CurrentTags.Any(x => x.IsChecked))
            {
                foreach (ITag tag in Model.CurrentTags.Where(x => x.IsChecked))
                {
                    foreach (IChannel channel in Model.Channels)
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
                Model.SelectedTag = null;
                foreach (IChannel channel in Model.Channels)
                {
                    channel.IsShowRow = true;
                }
            }
            Model.SelectedChannel = Model.Channels.First(x => x.IsShowRow);
        }

        private async Task Vacuumdb()
        {
            ISqLiteDatabase db = Model.BaseFactory.CreateSqLiteDatabase();
            long sizebefore = db.FileBase.Length;
            await db.VacuumAsync();
            long sizeafter = new FileInfo(db.FileBase.FullName).Length;
            Model.Info = string.Format("Database compacted (bytes): {0} -> {1}", sizebefore, sizeafter);
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
                    OpenDescription(Model.SelectedVideoItem);
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
                Clipboard.SetText(Model.SelectedVideoItem.MakeLink());
            }
            catch (Exception ex)
            {
                Model.Info = ex.Message;
            }
        }

        #endregion
    }
}
