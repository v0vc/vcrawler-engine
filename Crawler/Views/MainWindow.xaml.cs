using System;
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
            //await ViewModelLocator.MvViewModel.Model.FillChannels();

            await ViewModel.Model.FillChannels();

            if (ChannelsGrid.SelectedIndex >= 0) //focus
            {
                ChannelsGrid.UpdateLayout();
                var row = (DataGridRow)ChannelsGrid.ItemContainerGenerator.ContainerFromIndex(ChannelsGrid.SelectedIndex);
                row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
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

            await channel.SyncChannelAsync(true);

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
                await ch.FillChannelItemsDbAsync();

                foreach (IVideoItem item in ch.ChannelItems)
                {
                    item.IsHasLocalFileFound(ViewModel.Model.DirPath);
                    //item.ItemState = "LocalYes";
                    //item.DownloadPercentage = 50;
                }

                var pls = await ch.GetChannelPlaylistsAsync();

                foreach (var pl in pls)
                {
                    ch.ChannelPlaylists.Add(pl);
                }
            }
        }

        private async void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var mitem = sender as MenuItem;
            if (mitem == null) return;

            var item = ViewModel.Model.SelectedVideoItem;

            switch (mitem.CommandParameter.ToString())
            {

                case "Edit":

                    var edv = new EditDescriptionView
                    {
                        DataContext = ViewModel.Model.SelectedVideoItem,
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    edv.Show();

                    break;

                case "HD":

                    if (item == null) return;

                    if (string.IsNullOrEmpty(ViewModel.Model.YouPath))
                    {
                        MessageBox.Show("Please, select youtube-dl");
                        return;
                    }

                    //if (string.IsNullOrEmpty(ViewModel.Model.FfmegPath))
                    //{
                    //    MessageBox.Show("Please, select ffmpeg");
                    //    return;
                    //}

                    await item.DownloadItem(ViewModel.Model.YouPath, ViewModel.Model.DirPath, true);

                    break;

                case "Delete":

                    if (item == null) return;

                    if (item.IsHasLocalFile & !string.IsNullOrEmpty(item.LocalFilePath))
                    {
                        var result = MessageBox.Show("Are you sure to delete:" + Environment.NewLine + item.Title + "?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                        if (result == MessageBoxResult.OK)
                        {
                            var fn = new FileInfo(item.LocalFilePath);
                            try
                            {
                                fn.Delete();
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

            if (pls.Count <= pl.PlaylistItems.Count)
            {
                ViewModel.Model.Result = "Finished";
                return;
            }

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

                    if (vi.Duration == 0)
                        continue;

                    //vi.IsNewItem = true;

                    vi.IsShowRow = true;

                    ViewModel.Model.SelectedChannel.ChannelItems.Add(vi);

                    pl.PlaylistItems.Add(vi);

                    //if (vi.ParentID == ViewModel.Model.SelectedChannel.ID && !ViewModel.Model.SelectedChannel.ChannelItems.Select(x=>x.ID).Contains(id))
                    //    await vi.InsertItemAsync();
                }
            }

            foreach (var item in ViewModel.Model.SelectedChannel.ChannelItems)
            {
                item.IsShowRow = pl.PlaylistItems.Select(x => x.ID).Contains(item.ID);
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
            }
        }

        private void Item_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null)
                return;
            var item = row.Item as IVideoItem;
            if (item != null)
            {
                var mpath = ViewModel.Model.MpcPath;
                if (string.IsNullOrEmpty(mpath))
                    MessageBox.Show("Please, select MPC");
                else
                    item.RunItem(ViewModel.Model.MpcPath);
            }
        }

        private async void VideoItemSaveButton_onClick(object sender, RoutedEventArgs e)
        {
            var item = ViewModel.Model.SelectedVideoItem;
            if (item == null) return;
            if (item.IsHasLocalFile)
                item.RunItem(ViewModel.Model.MpcPath);
            else
            {
                if (!string.IsNullOrEmpty(ViewModel.Model.YouPath))
                    await item.DownloadItem(ViewModel.Model.YouPath, ViewModel.Model.DirPath, false);
                else
                    MessageBox.Show("Please, select youtube-dl");
            }
        }
    }
}
