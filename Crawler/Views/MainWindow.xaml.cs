// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Crawler.ViewModels;
using Interfaces.Factories;
using Interfaces.Models;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private GridLength _rememberWidth = GridLength.Auto;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        // [Inject]

        #region Properties

        public MainWindowViewModel ViewModel
        {
            get
            {
                return DataContext as MainWindowViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        #endregion

        #region Static Methods

        private static bool IsFfmegExist()
        {
            const string ff = "ffmpeg.exe";
            string values = Environment.GetEnvironmentVariable("PATH");
            return values != null && values.Split(';').Select(path => Path.Combine(path, ff)).Any(File.Exists);
        }

        #endregion

        #region Methods

        private async Task ConfirmDelete(MultiSelector dataGrid)
        {
            var sb = new StringBuilder();

            foreach (IChannel channel in dataGrid.SelectedItems)
            {
                if (channel.ID != "pop")
                {
                    sb.Append(channel.Title).Append(Environment.NewLine);
                }
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
                for (int i = dataGrid.SelectedItems.Count; i > 0; i--)
                {
                    var channel = (IChannel)dataGrid.SelectedItems[i - 1];
                    ViewModel.Model.Channels.Remove(channel);
                    await channel.DeleteChannelAsync();
                }

                if (ViewModel.Model.Channels.Any())
                {
                    ViewModel.Model.SelectedChannel = ViewModel.Model.Channels.First();
                }
            }
        }

        #endregion

        #region Event Handling

        private void BgvDoWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(async () => await ViewModel.Model.FillChannels()));
        }

        private void BgvRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                ViewModel.Model.SetStatus(3);
            }
            else
            {
                ViewModel.Model.SetStatus(0);

                if (ChannelsGrid.SelectedIndex >= 0)
                {
                    // focus
                    ChannelsGrid.UpdateLayout();
                    var row = (DataGridRow)ChannelsGrid.ItemContainerGenerator.ContainerFromIndex(ChannelsGrid.SelectedIndex);
                    if (row != null)
                    {
                        row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }
                }
            }
        }

        private async void Channel_OnClick(object sender, RoutedEventArgs e)
        {
            var mitem = sender as MenuItem;
            if (mitem == null)
            {
                return;
            }

            switch (mitem.CommandParameter.ToString())
            {
                case "Delete":

                    await ConfirmDelete(ChannelsGrid);

                    break;

                case "Edit":

                    ViewModel.AddNewItem(true);

                    break;

                case "Update":

                    ViewModel.Model.SetStatus(1);
                    if (ViewModel.Model.SelectedChannel.IsInWork)
                    {
                        return;
                    }

                    try
                    {
                        await ViewModel.Model.SelectedChannel.SyncChannelPlaylistsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        ViewModel.Model.SetStatus(3);
                    }

                    ViewModel.Model.SetStatus(2);

                    break;

                case "Related":

                    try
                    {
                        await ViewModel.Model.FindRelatedChannels(ViewModel.Model.SelectedChannel);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        ViewModel.Model.SetStatus(2);
                    }

                    break;

                case "Subscribe":

                    IChannel channel = ViewModel.Model.SelectedChannel;
                    if (channel.IsInWork)
                    {
                        return;
                    }

                    ViewModel.Model.Channels.Add(channel);
                    await channel.InsertChannelItemsAsync();

                    break;

                case "Tags":

                    var etvm = new EditTagsViewModel
                    {
                        ParentChannel = ViewModel.Model.SelectedChannel,
                        CurrentTags = ViewModel.Model.CurrentTags,
                        Tags = ViewModel.Model.Tags,
                        Channels = ViewModel.Model.Channels
                    };

                    var etv = new EditTagsView
                    {
                        DataContext = etvm,
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        Title = string.Format("Tags: {0}", etvm.ParentChannel.Title)
                    };
                    etv.ShowDialog();

                    break;
            }
        }

        private async void Channel_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null)
            {
                return;
            }

            var channel = row.Item as IChannel;
            if (channel == null || channel.IsInWork)
            {
                return;
            }

            ViewModel.Model.SetStatus(1);

            if (channel.ID != "pop")
            {
                await ViewModel.Model.SyncChannel(channel);
            }
            else
            {
                if (channel.ChannelItems.Any())
                {
                    // чтоб не удалять список отдельных закачек, но почистить прошлые популярные
                    for (int i = channel.ChannelItems.Count; i > 0; i--)
                    {
                        if (
                            !(channel.ChannelItems[i - 1].ItemState == "LocalYes"
                              || channel.ChannelItems[i - 1].ItemState == "Downloading"))
                        {
                            channel.ChannelItems.RemoveAt(i - 1);
                        }
                    }
                }

                IEnumerable<IVideoItem> lst = await channel.GetPopularItemsNetAsync(ViewModel.Model.SelectedCountry, 30);

                foreach (IVideoItem item in lst)
                {
                    channel.AddNewItem(item, false);
                    item.IsHasLocalFileFound(ViewModel.Model.DirPath);
                }
            }

            ViewModel.Model.SetStatus(2);
        }

        private async void Channel_OnToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var image = e.Source as Image;
            if (image == null)
            {
                return;
            }

            var channel = image.DataContext as IChannel;
            if (channel != null && string.IsNullOrEmpty(channel.SubTitle))
            {
                await channel.FillChannelDescriptionAsync();
            }
        }

        private async void ChannelsGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
            {
                return;
            }

            var ch = e.AddedItems[0] as IChannel;

            if (ch == null)
            {
                return;
            }

            ViewModel.Model.Filter = string.Empty;
            ViewModel.Model.IsExpand = false;

            if (ViewModel.Model.RelatedChannels.Any() && !ViewModel.Model.RelatedChannels.Contains(ch))
            {
                foreach (IChannel channel in ViewModel.Model.RelatedChannels)
                {
                    channel.ChannelItems.Clear();
                }

                ViewModel.Model.RelatedChannels.Clear();
            }

            foreach (IVideoItem item in ch.ChannelItems)
            {
                item.IsShowRow = true;
            }

            // есть новые элементы после синхронизации
            bool isHasNewFromSync = ch.ChannelItems.Any() && ch.ChannelItems.Count == ch.ChannelItems.Count(x => x.IsNewItem);

            // заполняем только если либо ничего нет, либо одни новые
            if ((!ch.ChannelItems.Any() & !ch.IsDownloading) || isHasNewFromSync)
            {
                if (isHasNewFromSync)
                {
                    List<string> lstnew = ch.ChannelItems.Select(x => x.ID).ToList();
                    ch.ChannelItems.Clear();
                    await ch.FillChannelItemsDbAsync(ViewModel.Model.DirPath);
                    foreach (IVideoItem item in from item in ch.ChannelItems from id in lstnew.Where(id => item.ID == id) select item)
                    {
                        item.IsNewItem = true;
                    }
                }
                else
                {
                    await ch.FillChannelItemsDbAsync(ViewModel.Model.DirPath);
                }

                if (ch.ChannelItems.Any())
                {
                    ch.PlaylistCount = await ch.GetChannelPlaylistCountDbAsync();
                }
                else
                {
                    // нет в базе = related channel
                    ViewModel.Model.SetStatus(1);
                    ch.IsInWork = true;
                    IEnumerable<IVideoItem> lst = await ch.GetChannelItemsNetAsync(0);
                    foreach (IVideoItem item in lst)
                    {
                        ch.AddNewItem(item, false);
                    }
                    ch.IsInWork = false;
                    ViewModel.Model.SetStatus(0);
                }
            }

            ViewModel.Model.Filterlist.Clear();
        }

        private void CheckBoxTag_OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (IChannel channel in ViewModel.Model.Channels)
            {
                channel.IsShowRow = false;
            }

            if (ViewModel.Model.CurrentTags.Any(x => x.IsChecked))
            {
                foreach (ITag tag in ViewModel.Model.CurrentTags.Where(x => x.IsChecked))
                {
                    foreach (IChannel channel in ViewModel.Model.Channels)
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
                foreach (IChannel channel in ViewModel.Model.Channels)
                {
                    channel.IsShowRow = true;
                }
            }
        }

        private async void ComboBoxTags_OnDropDownOpened(object sender, EventArgs e)
        {
            if (ViewModel.Model.CurrentTags.Any())
            {
                return;
            }

            foreach (IChannel ch in ViewModel.Model.Channels)
            {
                IEnumerable<ITag> tags = await ch.GetChannelTagsAsync();
                foreach (ITag tag in tags)
                {
                    ch.ChannelTags.Add(tag);
                    if (!ViewModel.Model.CurrentTags.Select(x => x.Title).Contains(tag.Title))
                    {
                        ViewModel.Model.CurrentTags.Add(tag);
                    }
                }
            }
        }

        private void CurrentTag_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.Model.SelectedTag == null)
            {
                return;
            }

            foreach (IChannel channel in ViewModel.Model.Channels)
            {
                channel.IsShowRow = true;
                if (!channel.ChannelTags.Select(x => x.Title).Contains(ViewModel.Model.SelectedTag.Title))
                {
                    channel.IsShowRow = false;
                }
            }
        }

        private void DataGridCellGotFocus(object sender, RoutedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null)
            {
                return;
            }

            var ch = grid.CurrentItem as IChannel;
            if (ch == null)
            {
                return;
            }

            ViewModel.Model.SelectedChannel = ch;
        }

        private async void DataGridPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
            {
                return;
            }
            if (e.Key == Key.Delete)
            {
                await ConfirmDelete(dataGrid);
            }
            if (e.Key == Key.Enter)
            {
                var channel = dataGrid.SelectedItem as IChannel;
                if (channel != null && channel.ID == "pop")
                {
                    await ViewModel.Model.Search();
                }
            }
        }

        private void GridCollapsed(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid == null)
            {
                return;
            }
            _rememberWidth = grid.ColumnDefinitions[1].Width;
            grid.ColumnDefinitions[1].Width = GridLength.Auto;
        }

        private void GridExpanded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid != null)
            {
                grid.ColumnDefinitions[1].Width = _rememberWidth;
            }
        }

        private async void Item_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null)
            {
                return;
            }

            var item = row.Item as IVideoItem;
            if (item == null)
            {
                return;
            }

            string mpath = ViewModel.Model.MpcPath;
            if (string.IsNullOrEmpty(mpath))
            {
                MessageBox.Show("Please, select MPC");
            }
            else
            {
                await item.RunItem(ViewModel.Model.MpcPath);
            }
        }

        private async void MainMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var mitem = sender as MenuItem;
            if (mitem == null)
            {
                return;
            }

            switch (mitem.CommandParameter.ToString())
            {
                case "Backup":

                    await ViewModel.Backup();

                    break;

                case "Restore":

                    await ViewModel.Restore();

                    break;

                case "Exit":

                    Close();

                    break;

                case "Settings":

                    ViewModel.OpenSettings();

                    break;

                case "Vacuum":

                    await ViewModel.Vacuumdb();

                    break;

                case "ShowAll":

                    ViewModel.Model.ShowAllChannels();

                    break;

                case "Link":

                    ViewModel.OpenAddLink();

                    break;

                case "About":

                    MessageBox.Show("by v0v © 2015", "About", MessageBoxButton.OK, MessageBoxImage.Information);

                    break;
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // await ViewModel.Model.FillChannels();
            ViewModel.Model.SetStatus(1);
            using (var bgv = new BackgroundWorker())
            {
                bgv.DoWork += BgvDoWork;
                bgv.RunWorkerCompleted += BgvRunWorkerCompleted;
                bgv.RunWorkerAsync();
            }
        }

        private async void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var mitem = sender as MenuItem;
            if (mitem == null)
            {
                return;
            }

            switch (mitem.CommandParameter.ToString())
            {
                case "Link":
                    try
                    {
                        Clipboard.SetText(ViewModel.Model.SelectedVideoItem.MakeLink());
                    }
                    catch (Exception ex)
                    {
                        ViewModel.Model.Info = ex.Message;
                    }

                    break;

                case "Edit":
                    var edv = new EditDescriptionView
                    {
                        DataContext = ViewModel.Model.SelectedVideoItem,
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    edv.Show();

                    break;

                case "Audio":

                    if (string.IsNullOrEmpty(ViewModel.Model.YouPath))
                    {
                        MessageBox.Show("Please, select youtube-dl");
                        return;
                    }

                    ViewModel.Model.SelectedChannel.IsDownloading = true;
                    await ViewModel.Model.SelectedVideoItem.DownloadItem(ViewModel.Model.YouPath, ViewModel.Model.DirPath, false, true);

                    break;

                case "HD":
                    if (string.IsNullOrEmpty(ViewModel.Model.YouPath))
                    {
                        MessageBox.Show("Please, select youtube-dl");
                        return;
                    }

                    if (IsFfmegExist())
                    {
                        ViewModel.Model.SelectedChannel.IsDownloading = true;
                        await
                            ViewModel.Model.SelectedVideoItem.DownloadItem(ViewModel.Model.YouPath, ViewModel.Model.DirPath, true, false);
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

                case "Delete":
                    var sb = new StringBuilder();

                    foreach (IVideoItem item in videoGrid.SelectedItems)
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
                        for (int i = videoGrid.SelectedItems.Count; i > 0; i--)
                        {
                            var item = videoGrid.SelectedItems[i - 1] as IVideoItem;

                            if (item == null)
                            {
                                continue;
                            }

                            var fn = new FileInfo(item.LocalFilePath);
                            try
                            {
                                fn.Delete();
                                await item.Log(string.Format("Deleted: {0}", item.LocalFilePath));
                                item.LocalFilePath = string.Empty;
                                item.IsHasLocalFile = false;
                                item.ItemState = "LocalNo";
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                    }

                    break;

                case "Subscribe":
                    if (ViewModel.Model.SelectedChannel.ID != "pop")
                    {
                        // этот канал по-любому есть - даже проверять не будем)
                        MessageBox.Show("Has already");
                        return;
                    }

                    await ViewModel.Model.AddNewChannel(ViewModel.Model.SelectedVideoItem.MakeLink());

                    break;
            }
        }

        private async void MenuItem_OnSubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Model.SelectedVideoItem.VideoItemChapters.Any())
            {
                await ViewModel.Model.SelectedVideoItem.FillChapters();
            }
        }

        private async void PlayListExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            IChannel ch = ViewModel.Model.SelectedChannel;
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

        private void Playlist_OnClick(object sender, RoutedEventArgs e)
        {
            var mitem = sender as MenuItem;
            if (mitem == null)
            {
                return;
            }
            switch (mitem.CommandParameter.ToString())
            {
                case "Link":

                    IPlaylist pl = ViewModel.Model.SelectedPlaylist;
                    if (pl != null)
                    {
                        try
                        {
                            string link = string.Format("https://www.youtube.com/playlist?list={0}", pl.ID);
                            Clipboard.SetText(link);
                        }
                        catch (Exception ex)
                        {
                            ViewModel.Model.Info = ex.Message;
                        }
                    }

                    break;

                case "Download":

                    break;
            }
        }

        private async void Playlist_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null)
            {
                return;
            }

            var pl = row.Item as IPlaylist;

            if (pl == null)
            {
                return;
            }

            ViewModel.Model.SetStatus(1);

            IEnumerable<string> pls = await pl.GetPlaylistItemsIdsListNetAsync();

            IVideoItemFactory vf = ViewModel.Model.BaseFactory.CreateVideoItemFactory();

            pl.PlaylistItems.Clear();

            foreach (IVideoItem item in ViewModel.Model.SelectedChannel.ChannelItems)
            {
                item.IsShowRow = false;
            }

            foreach (string id in pls.Where(id => !pl.PlaylistItems.Select(x => x.ID).Contains(id)))
            {
                IVideoItem vi = await vf.GetVideoItemNetAsync(id);

                ViewModel.Model.SelectedChannel.AddNewItem(vi, false);

                pl.PlaylistItems.Add(vi);
            }

            ViewModel.Model.SetStatus(2);
        }

        private async void PlaylistsGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
            {
                return;
            }

            var pl = e.AddedItems[0] as IPlaylist;
            if (pl == null)
            {
                return;
            }

            IEnumerable<string> lstv = (await pl.GetPlaylistItemsIdsListDbAsync()).ToList();
            if (!lstv.Any())
            {
                foreach (IVideoItem item in ViewModel.Model.SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = false;
                }

                foreach (IVideoItem item in ViewModel.Model.SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = pl.PlaylistItems.Select(x => x.ID).Contains(item.ID);
                }

                return;
            }

            pl.PlaylistItems.Clear();

            foreach (IVideoItem item in ViewModel.Model.SelectedChannel.ChannelItems)
            {
                item.IsShowRow = lstv.Contains(item.ID);
                if (item.IsShowRow)
                {
                    pl.PlaylistItems.Add(item);
                }
            }

            ViewModel.Model.Filterlist.Clear();

            // VideoGrid.UpdateLayout();
        }

        private void VideoGrid_OnSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Column.SortDirection = e.Column.SortDirection ?? ListSortDirection.Ascending;
        }

        private void VideoImage_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var edv = new EditDescriptionView
            {
                DataContext = ViewModel.Model.SelectedVideoItem,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            edv.Show();
        }

        private async void VideoItemSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            IVideoItem item = ViewModel.Model.SelectedVideoItem;
            if (item == null)
            {
                return;
            }
            if (item.IsHasLocalFile)
            {
                if (!string.IsNullOrEmpty(ViewModel.Model.MpcPath))
                {
                    await item.RunItem(ViewModel.Model.MpcPath);
                }
                else
                {
                    MessageBox.Show("Path to MPC is not set, please check");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(ViewModel.Model.YouPath))
                {
                    var fn = new FileInfo(ViewModel.Model.YouPath);
                    if (fn.Exists)
                    {
                        ViewModel.Model.SelectedChannel.IsDownloading = true;
                        await item.DownloadItem(fn.FullName, ViewModel.Model.DirPath, false, false);
                    }
                    else
                    {
                        MessageBox.Show("Please, check path to youtube-dl");
                    }
                }
                else
                {
                    MessageBox.Show("Please, select youtube-dl");
                }
            }
        }

        private async void VideoItem_OnToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var image = e.Source as Image;
            if (image == null)
            {
                return;
            }
            var item = image.DataContext as IVideoItem;
            if (item != null && string.IsNullOrEmpty(item.Description))
            {
                await item.FillDescriptionAsync();
            }
        }

        #endregion
    }
}
