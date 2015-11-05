// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Crawler.ViewModels;
using Interfaces.Enums;
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

        #region Event Handling

        //private async void Channel_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    var row = sender as DataGridRow;
        //    if (row == null)
        //    {
        //        return;
        //    }

        //    var channel = row.Item as IChannel;
        //    if (channel == null || channel.IsInWork)
        //    {
        //        return;
        //    }

        //    ViewModel.Model.SetStatus(1);
        //    if (channel.ID != "pop")
        //    {
        //        await ViewModel.Model.SyncChannel(channel);
        //    }
        //    else
        //    {
        //        if (channel.ChannelItems.Any())
        //        {
        //            // чтоб не удалять список отдельных закачек, но почистить прошлые популярные
        //            for (int i = channel.ChannelItems.Count; i > 0; i--)
        //            {
        //                if (
        //                    !(channel.ChannelItems[i - 1].State == ItemState.LocalYes
        //                      || channel.ChannelItems[i - 1].State == ItemState.Downloading))
        //                {
        //                    channel.ChannelItems.RemoveAt(i - 1);
        //                }
        //            }
        //        }

        //        try
        //        {
        //            IEnumerable<IVideoItem> lst = await channel.GetPopularItemsNetAsync(ViewModel.Model.SelectedCountry, 30);
        //            foreach (IVideoItem item in lst)
        //            {
        //                channel.AddNewItem(item, false);
        //                item.IsHasLocalFileFound(ViewModel.Model.DirPath);
        //                if (ViewModel.Model.Channels.Select(x => x.ID).Contains(item.ParentID))
        //                {
        //                    // подсветим видео, если канал уже есть в подписке
        //                    item.IsNewItem = true;
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(ex.Message);
        //            ViewModel.Model.SetStatus(3);
        //        }
        //    }

        //    ViewModel.Model.SetStatus(0);
        //}

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
                ViewModel.Model.SelectedTag = null;
                foreach (IChannel channel in ViewModel.Model.Channels)
                {
                    channel.IsShowRow = true;
                }
            }
            ViewModel.Model.SelectedChannel = ViewModel.Model.Channels.First(x => x.IsShowRow);
        }

        private async void ComboBoxTags_OnDropDownOpened(object sender, EventArgs e)
        {
            if (ViewModel.Model.CurrentTags.Any())
            {
                return;
            }

            var tmptags = new List<ITag>();
            foreach (IChannel ch in ViewModel.Model.Channels)
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
                ViewModel.Model.CurrentTags.Add(tag);
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
            foreach (ITag tag in ViewModel.Model.CurrentTags)
            {
                if (tag.Title != ViewModel.Model.SelectedTag.Title && tag.IsChecked)
                {
                    tag.IsChecked = false;
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

        private async void MenuItem_OnSubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Model.SelectedVideoItem.Subtitles.Any())
            {
                await ViewModel.Model.SelectedVideoItem.FillSubtitles();
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
                IVideoItem vi = await vf.GetVideoItemNetAsync(id, pl.Site);

                ViewModel.Model.SelectedChannel.AddNewItem(vi, false);

                pl.PlaylistItems.Add(vi);
            }

            ViewModel.Model.SetStatus(0);
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

        private async void VideoGrid_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange <= 0)
            {
                return;
            }
            if (ViewModel.Model.SelectedChannel.ChannelItemsCount > ViewModel.Model.SelectedChannel.ChannelItems.Count)
            {
                await
                    ViewModel.Model.SelectedChannel.FillChannelItemsDbAsync(ViewModel.Model.DirPath, 
                        ViewModel.Model.SelectedChannel.ChannelItemsCount - ViewModel.Model.SelectedChannel.ChannelItems.Count, 
                        ViewModel.Model.SelectedChannel.ChannelItems.Count);
            }
        }

        private void VideoGrid_OnSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Column.SortDirection = e.Column.SortDirection ?? ListSortDirection.Ascending;
        }

        //private async void VideoItem_OnClick(object sender, RoutedEventArgs e)
        //{
        //    var mitem = sender as MenuItem;
        //    if (mitem == null)
        //    {
        //        return;
        //    }

        //    switch (mitem.CommandParameter.ToString())
        //    {
        //        case "Link":
        //            try
        //            {
        //                Clipboard.SetText(ViewModel.Model.SelectedVideoItem.MakeLink());
        //            }
        //            catch (Exception ex)
        //            {
        //                ViewModel.Model.Info = ex.Message;
        //            }

        //            break;

        //        case "Edit":
        //            var edv = new EditDescriptionView
        //            {
        //                DataContext = ViewModel.Model.SelectedVideoItem, 
        //                Owner = Application.Current.MainWindow, 
        //                WindowStartupLocation = WindowStartupLocation.CenterOwner
        //            };

        //            edv.Show();

        //            break;

        //        case "Audio":

        //            if (string.IsNullOrEmpty(ViewModel.Model.YouPath))
        //            {
        //                MessageBox.Show("Please, select youtube-dl");
        //                return;
        //            }

        //            ViewModel.Model.SelectedChannel.IsDownloading = true;
        //            await ViewModel.Model.SelectedVideoItem.DownloadItem(ViewModel.Model.YouPath, ViewModel.Model.DirPath, false, true);

        //            break;

        //        case "HD":
        //            if (string.IsNullOrEmpty(ViewModel.Model.YouPath))
        //            {
        //                MessageBox.Show("Please, select youtube-dl");
        //                return;
        //            }

        //            if (IsFfmegExist())
        //            {
        //                ViewModel.Model.SelectedChannel.IsDownloading = true;
        //                await
        //                    ViewModel.Model.SelectedVideoItem.DownloadItem(ViewModel.Model.YouPath, ViewModel.Model.DirPath, true, false);
        //            }
        //            else
        //            {
        //                var ff = new FfmpegView
        //                {
        //                    Owner = Application.Current.MainWindow, 
        //                    WindowStartupLocation = WindowStartupLocation.CenterOwner
        //                };

        //                ff.ShowDialog();
        //            }

        //            break;

        //        case "Delete":
        //            var sb = new StringBuilder();

        //            foreach (IVideoItem item in videoGrid.SelectedItems)
        //            {
        //                if (item.IsHasLocalFile & !string.IsNullOrEmpty(item.LocalFilePath))
        //                {
        //                    sb.Append(item.Title).Append(Environment.NewLine);
        //                }
        //            }

        //            if (sb.Length == 0)
        //            {
        //                return;
        //            }

        //            MessageBoxResult result = MessageBox.Show("Are you sure to delete:" + Environment.NewLine + sb + "?", 
        //                "Confirm", 
        //                MessageBoxButton.OKCancel, 
        //                MessageBoxImage.Information);

        //            if (result == MessageBoxResult.OK)
        //            {
        //                for (int i = videoGrid.SelectedItems.Count; i > 0; i--)
        //                {
        //                    var item = videoGrid.SelectedItems[i - 1] as IVideoItem;

        //                    if (item == null)
        //                    {
        //                        continue;
        //                    }

        //                    var fn = new FileInfo(item.LocalFilePath);
        //                    try
        //                    {
        //                        fn.Delete();
        //                        await item.Log(string.Format("Deleted: {0}", item.LocalFilePath));
        //                        item.LocalFilePath = string.Empty;
        //                        item.IsHasLocalFile = false;
        //                        item.State = ItemState.LocalNo;
        //                        item.Subtitles.Clear();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        MessageBox.Show(ex.Message);
        //                    }
        //                }
        //            }

        //            break;

        //        case "Subscribe":
        //            if (ViewModel.Model.SelectedChannel.ID != "pop")
        //            {
        //                // этот канал по-любому есть - даже проверять не будем)
        //                MessageBox.Show("Has already");
        //                return;
        //            }

        //            await
        //                ViewModel.Model.AddNewChannel(ViewModel.Model.SelectedVideoItem.MakeLink(), ViewModel.Model.SelectedChannel.Site);

        //            break;
        //    }
        //}

        private async void VideoItem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
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

        #endregion
    }
}
