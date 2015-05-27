﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Models.BO;
using SitesAPI;

namespace Crawler.Views
{
    /// <summary>
    /// Interaction logic for EditDescriptionView.xaml
    /// </summary>
    public partial class EditDescriptionView : Window
    {
        public EditDescriptionView()
        {
            InitializeComponent();
            KeyDown += AddChanelView_KeyDown;
        }

        private void AddChanelView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                KeyDown -= AddChanelView_KeyDown;
                Close();
            }
        }

        private async void EditDescriptionView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var context = (DataContext as VideoItem);
            if (context == null) return;

            var id = context.ID;
            var link = string.Format("http://img.youtube.com/vi/{0}/0.jpg", id);

            context.LargeThumb = await SiteHelper.GetStreamFromUrl(link);
        }
    }
}