using System;
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
using Interfaces.Models;
using Models.BO;

namespace Crawler.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GridLength _rememberWidth = GridLength.Auto;
        //[Inject]
        public MainWindowViewModel ViewModel
        {
            get { return DataContext as MainWindowViewModel; }
            set { DataContext = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            using (var bgv = new BackgroundWorker())
            {
                bgv.DoWork += bgv_DoWork;
                bgv.RunWorkerCompleted += bgv_RunWorkerCompleted;
                bgv.RunWorkerAsync();
            }
            //await ViewModel.Model.FillChannels();
        }

        void bgv_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ViewModel.Model.Result = "Ready";
            //if (ChannelsGrid.SelectedIndex >= 0) //focus
            //{
            //    ChannelsGrid.UpdateLayout();
            //    var row = (DataGridRow)ChannelsGrid.ItemContainerGenerator.ContainerFromIndex(ChannelsGrid.SelectedIndex);
            //    if (row != null)
            //        row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            //}
        }

        void bgv_DoWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(async () => await ViewModel.Model.FillChannels()));
        }

        private async void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid != null && dataGrid.SelectedItems.Count > 0 && e.Key == Key.Delete)
            {
                await ConfirmDelete(dataGrid);
            }
        }

        private async void Channel_OnClick(object sender, RoutedEventArgs e)
        {
            var mitem = sender as MenuItem;
            if (mitem == null) return;

            switch (mitem.CommandParameter.ToString())
            {
                case "Delete":

                    await ConfirmDelete(ChannelsGrid);

                    break;

                case "Edit":

                    ViewModel.AddNewItem(true);

                    break;

                case "Update":

                    ViewModel.Model.Result = "Working..";

                    try
                    {
                        await ViewModel.Model.SelectedChannel.SyncChannelPlaylistsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    ViewModel.Model.Result = "Finished";

                    break;
            }
        }

        private async Task ConfirmDelete(MultiSelector dataGrid)
        {
            var sb = new StringBuilder();

            foreach (Channel channel in dataGrid.SelectedItems)
            {
                sb.Append(channel.Title).Append(Environment.NewLine);
            }

            var result = MessageBox.Show("Delete:" + Environment.NewLine + sb + "?", "Confirm",
                MessageBoxButton.OKCancel, MessageBoxImage.Information);

            if (result == MessageBoxResult.OK)
            {
                for (int i = dataGrid.SelectedItems.Count; i > 0; i--)
                {
                    var channel = (Channel) dataGrid.SelectedItems[i - 1];
                    ViewModel.Model.Channels.Remove(channel);
                    await channel.DeleteChannelAsync();
                }

                if (ViewModel.Model.Channels.Any())
                    ViewModel.Model.SelectedChannel = ViewModel.Model.Channels.First();
            }
        }

        private async void Channel_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null)
                return;

            var channel = row.Item as IChannel;
            if (channel == null) return;

            ViewModel.Model.Result = "Working..";

            if (channel.ID != "pop")
            {
                await ViewModel.Model.SyncChannel(channel);
            }
            else
            {
                #region Popular

                if (channel.ChannelItems.Any())
                    channel.ChannelItems.Clear();

                var lst = await channel.GetPopularItemsNetAsync(ViewModel.Model.SelectedCountry, 30);

                foreach (IVideoItem item in lst)
                {
                    channel.AddNewItem(item, false);
                    //item.IsShowRow = true;
                    item.IsHasLocalFileFound(ViewModel.Model.DirPath);
                    //channel.ChannelItems.Add(item);
                    //channel.CountNew = channel.ChannelItems.Count;
                }

                #endregion
            }

            ViewModel.Model.Result = "Finished";
        }

        private async void ChannelsGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;

            var ch = e.AddedItems[0] as Channel;

            if (ch == null) return;

            foreach (var item in ch.ChannelItems)
            {
                item.IsShowRow = true;
            }

            if (!ch.ChannelItems.Any() || ch.ChannelItems.Any(x => x.IsNewItem)) //заполняем только если либо ничего нет, либо одни новые
            {
                await ch.FillChannelItemsDbAsync(ViewModel.Model.DirPath);

                var pls = await ch.GetChannelPlaylistsAsync();

                foreach (var pl in pls)
                {
                    ch.ChannelPlaylists.Add(pl);
                }
            }
            //ViewModel.Model.Filter = string.Empty;
            ViewModel.Model.Filterlist.Clear();
        }

        private async void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var mitem = sender as MenuItem;
            if (mitem == null) return;

            switch (mitem.CommandParameter.ToString())
            {
                case "Link":

                    #region Link

                    var vid = ViewModel.Model.SelectedVideoItem;
                    if (vid != null)
                    {
                        var link = string.Format("https://www.youtube.com/watch?v={0}", vid.ID);
                        Clipboard.SetText(link);
                    }

                    #endregion

                    break;

                case "Edit":

                    #region Edit

                    var edv = new EditDescriptionView
                    {
                        DataContext = ViewModel.Model.SelectedVideoItem,
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    edv.Show();

                    #endregion

                    break;

                case "HD":

                    #region HD

                    if (string.IsNullOrEmpty(ViewModel.Model.YouPath))
                    {
                        MessageBox.Show("Please, select youtube-dl");
                        return;
                    }

                    if (IsFfmegExist())
                    {
                        ViewModel.Model.SelectedChannel.IsDownloading = true;
                        await
                            ViewModel.Model.SelectedVideoItem.DownloadItem(ViewModel.Model.YouPath,
                                ViewModel.Model.DirPath, true);
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

                    #endregion

                    break;

                case "Delete":

                    #region Delete

                    var sb = new StringBuilder();

                    foreach (IVideoItem item in VideoGrid.SelectedItems)
                    {
                        if (item.IsHasLocalFile & !string.IsNullOrEmpty(item.LocalFilePath))
                        {
                            sb.Append(item.Title).Append(Environment.NewLine);
                        }
                    }

                    var result = MessageBox.Show("Are you sure to delete:" + Environment.NewLine + sb + "?", "Confirm",
                        MessageBoxButton.OKCancel, MessageBoxImage.Information);

                    if (result == MessageBoxResult.OK)
                    {
                        for (var i = VideoGrid.SelectedItems.Count; i > 0; i--)
                        {
                            var item = VideoGrid.SelectedItems[i - 1] as VideoItem;

                            if (item == null) continue;

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

                    #endregion

                    break;
            }
        }

        private async void PlaylistsGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;

            var pl = e.AddedItems[0] as Playlist;

            if (pl == null) return;

            var lstv = await pl.GetPlaylistItemsIdsListDbAsync();

            if (!lstv.Any())
            {
                foreach (var item in ViewModel.Model.SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = false;
                }

                foreach (var item in ViewModel.Model.SelectedChannel.ChannelItems)
                {
                    item.IsShowRow = pl.PlaylistItems.Select(x => x.ID).Contains(item.ID);
                }

                return;
            }

            pl.PlaylistItems.Clear();

            foreach (var item in ViewModel.Model.SelectedChannel.ChannelItems)
            {
                item.IsShowRow = lstv.Contains(item.ID);
                if (item.IsShowRow)
                    pl.PlaylistItems.Add(item);
            }

            ViewModel.Model.Filterlist.Clear();
            //VideoGrid.UpdateLayout();
        }

        private async void Playlist_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null)
                return;

            var pl = row.Item as IPlaylist;

            if (pl == null) return;

            ViewModel.Model.Result = "Working..";

            var pls = await pl.GetPlaylistItemsIdsListNetAsync();

            var vf = ViewModel.Model.BaseFactory.CreateVideoItemFactory();

            pl.PlaylistItems.Clear();

            foreach (var item in ViewModel.Model.SelectedChannel.ChannelItems)
            {
                item.IsShowRow = false;
            }

            foreach (string id in pls)
            {
                if (!pl.PlaylistItems.Select(x => x.ID).Contains(id))
                {
                    var vi = await vf.GetVideoItemNetAsync(id);

                    ViewModel.Model.SelectedChannel.AddNewItem(vi, false);

                    pl.PlaylistItems.Add(vi);
                }
            }

            ViewModel.Model.Result = "Finished";
        }

        private void Playlist_OnClick(object sender, RoutedEventArgs e)
        {
            var mitem = sender as MenuItem;
            if (mitem == null) return;
            switch (mitem.CommandParameter.ToString())
            {
                case "Link":

                    var pl = ViewModel.Model.SelectedPlaylist;
                    if (pl != null)
                    {
                        var link = string.Format("https://www.youtube.com/playlist?list={0}", pl.ID);
                        Clipboard.SetText(link);
                    }

                    break;

                case "Download":



                    break;
            }
        }

        private void MainMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var mitem = sender as MenuItem;
            if (mitem == null) return;

            switch (mitem.CommandParameter.ToString())
            {
                case "Exit":

                    Close();

                    break;

                case "Settings":

                    ViewModel.OpenSettings();

                    break;

                case "Link":

                    ViewModel.OpenAddLink();

                    break;

                case "About":

                    MessageBox.Show("by v0v © 2015", "About", MessageBoxButton.OK, MessageBoxImage.Information);

                    break;
            }
        }

        private async void Item_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null)
                return;

            var item = row.Item as IVideoItem;
            if (item == null) 
                return;

            var mpath = ViewModel.Model.MpcPath;
            if (string.IsNullOrEmpty(mpath))
                MessageBox.Show("Please, select MPC");
            else
                await item.RunItem(ViewModel.Model.MpcPath);
        }

        private async void VideoItemSaveButton_onClick(object sender, RoutedEventArgs e)
        {
            var item = ViewModel.Model.SelectedVideoItem;
            if (item == null) return;
            if (item.IsHasLocalFile)
                await item.RunItem(ViewModel.Model.MpcPath);
            else
            {
                if (!string.IsNullOrEmpty(ViewModel.Model.YouPath))
                {
                    ViewModel.Model.SelectedChannel.IsDownloading = true;
                    await item.DownloadItem(ViewModel.Model.YouPath, ViewModel.Model.DirPath, false);
                }
                else
                    MessageBox.Show("Please, select youtube-dl");
            }
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

        private static bool IsFfmegExist()
        {
            const string ff = "ffmpeg.exe";
            var values = Environment.GetEnvironmentVariable("PATH");
            return values != null && values.Split(';').Select(path => Path.Combine(path, ff)).Any(File.Exists);
        }

        private void Grid_Collapsed(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid != null)
            {
                _rememberWidth = grid.ColumnDefinitions[1].Width;
                grid.ColumnDefinitions[1].Width = GridLength.Auto;
            }
        }

        private void Grid_Expanded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid != null)
            {
                grid.ColumnDefinitions[1].Width = _rememberWidth;
            }
        }
    }
}
