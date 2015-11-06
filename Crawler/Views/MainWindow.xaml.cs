// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        #region Event Handling

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

        #endregion
    }
}
