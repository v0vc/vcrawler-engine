using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Crawler.Properties;
using Crawler.ViewModels;
using Extensions;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        public SettingsView()
        {
            InitializeComponent();
            KeyDown += SettingsView_KeyDown;
        }

        public MainWindowViewModel ViewModel
        {
            get { return DataContext as MainWindowViewModel; }
            set { DataContext = value; }
        }

        private void SettingsView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                KeyDown -= SettingsView_KeyDown;
                Close();
            }
        }

        private void UpdateButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ViewModel.Model.YouPath))
            {
                return;
            }
            var link = (string)Settings.Default["pathToYoudl"];

            ViewModel.Model.YouHeader = "Youtube-dl (update in progress..)";

            using (var client = new WebClient())
            {
                client.DownloadProgressChanged += client_DownloadProgressChanged;
                client.DownloadFileCompleted += client_DownloadFileCompleted;
                client.DownloadFileAsync(new Uri(link), ViewModel.Model.YouPath);
            }
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var bytesIn = double.Parse(e.BytesReceived.ToString(CultureInfo.InvariantCulture));
            var totalBytes = double.Parse(e.TotalBytesToReceive.ToString(CultureInfo.InvariantCulture));
            ViewModel.Model.PrValue = bytesIn / totalBytes * 100;
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ViewModel.Model.YouHeader = string.Format("Youtube-dl ({0})", 
                CommonExtensions.GetVersion(ViewModel.Model.YouPath, "--version").Trim());
            ViewModel.Model.PrValue = 0;
        }

        private void SettingsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ViewModel.Model.YouPath))
            {
                ViewModel.Model.YouHeader = "Youtube-dl";
            }
            else
            {
                ViewModel.Model.YouHeader = string.Format("Youtube-dl ({0})", CommonExtensions.GetVersion(ViewModel.Model.YouPath, "--version").Trim());
            }
        }
    }
}
