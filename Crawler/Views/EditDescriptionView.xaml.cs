// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Windows;
using System.Windows.Input;
using DataAPI;
using Interfaces.Models;

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
            KeyDown += AddChanelViewKeyDown;
        }

        #endregion

        #region Event Handling

        private void AddChanelViewKeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= AddChanelViewKeyDown;
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
