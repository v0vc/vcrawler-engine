// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Windows;
using System.Windows.Input;
using Interfaces.Models;
using SitesAPI;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for EditDescriptionView.xaml
    /// </summary>
    public partial class EditDescriptionView : Window
    {
        #region Constructors

        public EditDescriptionView()
        {
            InitializeComponent();
            KeyDown += AddChanelView_KeyDown;
        }

        #endregion

        #region Event Handling

        private void AddChanelView_KeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= AddChanelView_KeyDown;
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private async void EditDescriptionView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var context = DataContext as IVideoItem;
            if (context == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(context.Description))
            {
                await context.FillDescriptionAsync();
            }

            string id = context.ID;
            string link = string.Format("http://img.youtube.com/vi/{0}/0.jpg", id);

            context.LargeThumb = await SiteHelper.GetStreamFromUrl(link);
        }

        #endregion
    }
}
