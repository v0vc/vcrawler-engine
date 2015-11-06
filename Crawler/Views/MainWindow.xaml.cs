﻿// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Crawler.ViewModels;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private GridLength rememberWidth = GridLength.Auto;

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

        private void GridCollapsed(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid == null)
            {
                return;
            }
            rememberWidth = grid.ColumnDefinitions[1].Width;
            grid.ColumnDefinitions[1].Width = GridLength.Auto;
        }

        private void GridExpanded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid != null)
            {
                grid.ColumnDefinitions[1].Width = rememberWidth;
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
