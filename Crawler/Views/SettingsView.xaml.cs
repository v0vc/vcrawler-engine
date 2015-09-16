// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Crawler.Properties;
using Crawler.ViewModels;
using Extensions;
using Interfaces.Models;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        #region Constructors

        public SettingsView()
        {
            InitializeComponent();
            KeyDown += SettingsView_KeyDown;
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

        public static bool CheckForInternetConnection(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (Stream stream = client.OpenRead(url))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Event Handling

        private async void ButtonDeleteTag_OnClick(object sender, RoutedEventArgs e)
        {
            var tag = ((Button)e.Source).DataContext as ITag;
            if (tag != null)
            {
                MessageBoxResult result =
                    MessageBox.Show(string.Format("Are you sure to delete Tag:{0}<{1}>" + "?", Environment.NewLine, tag.Title), 
                        "Confirm", 
                        MessageBoxButton.OKCancel, 
                        MessageBoxImage.Information);

                if (result == MessageBoxResult.OK)
                {
                    ViewModel.Model.Tags.Remove(tag);
                    await tag.DeleteTagAsync();
                }
            }
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ViewModel.Model.YouHeader = string.Format("Youtube-dl ({0})", 
                CommonExtensions.GetConsoleOutput(ViewModel.Model.YouPath, "--version", true).Trim());
            ViewModel.Model.PrValue = 0;
            ViewModel.Model.IsIdle = true;
            ViewModel.Model.Info = e.Error == null ? "Youtube-dl has been updated" : e.Error.InnerException.Message;
            var webClient = sender as WebClient;
            if (webClient != null)
            {
                webClient.DownloadFileCompleted -= client_DownloadFileCompleted;
                webClient.DownloadProgressChanged -= client_DownloadProgressChanged;
            }
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString(CultureInfo.InvariantCulture));
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString(CultureInfo.InvariantCulture));
            ViewModel.Model.PrValue = bytesIn / totalBytes * 100;
        }

        private void SettingsView_KeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= SettingsView_KeyDown;
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void SettingsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ViewModel.Model.YouPath))
            {
                ViewModel.Model.YouHeader = "Youtube-dl";
            }
            else
            {
                ViewModel.Model.YouHeader = string.Format("Youtube-dl ({0})", 
                    CommonExtensions.GetConsoleOutput(ViewModel.Model.YouPath, "--version", true).Trim());
            }
        }

        private void UpdateButton_OnClick(object sender, RoutedEventArgs e)
        {
            var link = (string)Settings.Default["pathToYoudl"];

            if (string.IsNullOrEmpty(ViewModel.Model.YouPath))
            {
                ViewModel.Model.YouPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    link.Split('/').Last());
            }
            
            ViewModel.Model.IsIdle = false;

            ViewModel.Model.Info = CommonExtensions.GetConsoleOutput(ViewModel.Model.YouPath, "--rm-cache-dir", false);

            if (CheckForInternetConnection(link))
            {
                ViewModel.Model.YouHeader = "Youtube-dl (update in progress..)";

                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += client_DownloadProgressChanged;
                    client.DownloadFileCompleted += client_DownloadFileCompleted;
                    client.DownloadFileAsync(new Uri(link), ViewModel.Model.YouPath);
                }
            }
            else
            {
                ViewModel.Model.IsIdle = true;
                MessageBox.Show(link + " is not available");
            }
        }

        #endregion
    }
}
