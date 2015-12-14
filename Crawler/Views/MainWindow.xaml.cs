// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handling

        private void VideoGrid_OnSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Column.SortDirection = e.Column.SortDirection ?? ListSortDirection.Ascending;
        }

        #endregion
    }
}
