// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Forms;
using Crawler.Common;
using Crawler.Properties;
using Crawler.Views;
using Extensions;
using Interfaces.Enums;
using Interfaces.Models;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Crawler.ViewModels
{
    public class SettingsViewModel
    {
        #region Constants

        private const string exeFilter = "EXE files (*.exe)|*.exe";

        #endregion

        #region Static and Readonly Fields

        private readonly MainWindowViewModel _mv;

        #endregion

        #region Fields

        private RelayCommand addNewTagCommand;
        private RelayCommand openDirCommand;
        private RelayCommand updateYouDlCommand;

        #endregion

        #region Constructors

        public SettingsViewModel(MainWindowViewModel mv)
        {
            _mv = mv;
        }

        #endregion

        #region Properties

        public RelayCommand AddNewTagCommand
        {
            get
            {
                return addNewTagCommand ?? (addNewTagCommand = new RelayCommand(x => AddNewTag()));
            }
        }

        public string DirPath { get; set; }
        public string MpcPath { get; set; }

        public RelayCommand OpenDirCommand
        {
            get
            {
                return openDirCommand ?? (openDirCommand = new RelayCommand(OpenDir));
            }
        }

        public IList<ICred> SupportedCreds { get; set; }
        public ObservableCollection<ITag> Tags { get; private set; }

        public RelayCommand UpdateYouDlCommand
        {
            get
            {
                return updateYouDlCommand ?? (updateYouDlCommand = new RelayCommand(x => UpdateYouDl()));
            }
        }

        public string YouHeader { get; set; }
        public string YouPath { get; set; }

        #endregion

        #region Static Methods

        private static bool CheckForInternetConnection(string url)
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

        #region Methods

        private void AddNewTag()
        {
            var advm = new AddNewTagViewModel { Tag = _mv.Model.BaseFactory.CreateTagFactory().CreateTag(), Tags = Tags };
            var antv = new AddNewTagView
            {
                DataContext = advm, 
                Owner = Application.Current.MainWindow, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            antv.ShowDialog();
        }

        private void OpenDir(object obj)
        {
            var item = (OpenDirParam)obj;
            switch (item)
            {
                case OpenDirParam.DirPath:

                    var dlg = new FolderBrowserDialog();
                    DialogResult res = dlg.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        DirPath = dlg.SelectedPath;
                    }

                    break;
                case OpenDirParam.MpcPath:

                    var dlgm = new OpenFileDialog { Filter = exeFilter };
                    DialogResult resm = dlgm.ShowDialog();
                    if (resm == DialogResult.OK)
                    {
                        MpcPath = dlgm.FileName;
                    }

                    break;
                case OpenDirParam.YouPath:

                    var dlgy = new OpenFileDialog { Filter = exeFilter };
                    DialogResult resy = dlgy.ShowDialog();
                    if (resy == DialogResult.OK)
                    {
                        YouPath = dlgy.FileName;
                    }

                    break;
            }
        }

        private void UpdateYouDl()
        {
            var link = (string)Settings.Default["pathToYoudl"];

            if (string.IsNullOrEmpty(YouPath))
            {
                YouPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), link.Split('/').Last());
            }

            // ViewModel.Model.IsWorking = true;

            // ViewModel.Model.Info = CommonExtensions.GetConsoleOutput(ViewModel.Model.YouPath, "--rm-cache-dir", false);
            if (CheckForInternetConnection(link))
            {
                YouHeader = "Youtube-dl (update in progress..)";

                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += ClientDownloadProgressChanged;
                    client.DownloadFileCompleted += ClientDownloadFileCompleted;
                    client.DownloadFileAsync(new Uri(link), YouPath);
                }
            }
            else
            {
                // ViewModel.Model.IsWorking = false;
                MessageBox.Show(link + " is not available");
            }
        }

        #endregion

        #region Event Handling

        private void ClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            YouHeader = string.Format("Youtube-dl ({0})", CommonExtensions.GetConsoleOutput(YouPath, "--version", true).Trim());

            // ViewModel.Model.PrValue = 0;
            // ViewModel.Model.IsWorking = false;
            // ViewModel.Model.Info = e.Error == null ? "Youtube-dl has been updated" : e.Error.InnerException.Message;
            var webClient = sender as WebClient;
            if (webClient == null)
            {
                return;
            }
            webClient.DownloadFileCompleted -= ClientDownloadFileCompleted;
            webClient.DownloadProgressChanged -= ClientDownloadProgressChanged;
        }

        private void ClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString(CultureInfo.InvariantCulture));
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString(CultureInfo.InvariantCulture));

            // ViewModel.Model.PrValue = bytesIn / totalBytes * 100;
        }

        #endregion
    }
}
