// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Crawler.Common;
using Extensions;
using Interfaces.Enums;
using Interfaces.Factories;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class DownloadLinkViewModel
    {
        private readonly MainWindowViewModel mv;

        #region Fields

        private RelayCommand downloadLinkCommand;

        #endregion

        public DownloadLinkViewModel()
        {
            // for xaml
        }


        public DownloadLinkViewModel(MainWindowViewModel mv)
        {
            this.mv = mv;
        }

        #region Properties

        public RelayCommand DownloadLinkCommand
        {
            get
            {
                return downloadLinkCommand ?? (downloadLinkCommand = new RelayCommand(DownloadLink));
            }
        }

        public bool IsAudio { get; set; }
        public bool IsHd { get; set; }
        public string Link { get; set; }

        #endregion

        #region Methods

        private async void DownloadLink(object obj)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(Link))
            {
                return;
            }

            if (!CommonExtensions.IsValidUrl(Link))
            {
                MessageBox.Show("Can't parse URL");
                return;
            }

            if (string.IsNullOrEmpty(mv.Model.SettingsViewModel.YouPath))
            {
                MessageBox.Show("Please, select youtube-dl");
                return;
            }

            var regex = new Regex(CommonExtensions.YouRegex);
            Match match = regex.Match(Link);
            if (match.Success)
            {
                string id = match.Groups[1].Value;
                IVideoItem vi = await mv.Model.BaseFactory.CreateVideoItemFactory().GetVideoItemNetAsync(id, SiteType.YouTube);
                vi.ParentID = null;
                mv.Model.SelectedVideoItem = vi;
                mv.Model.SelectedChannel.AddNewItem(vi, true);

                await vi.DownloadItem(mv.Model.SettingsViewModel.YouPath, mv.Model.SettingsViewModel.DirPath, IsHd, IsAudio);
                vi.IsNewItem = true;
            }
            else
            {
                string param = string.Format("-o {0}\\%(title)s.%(ext)s {1} --no-check-certificate -i --console-title",
                    mv.Model.SettingsViewModel.DirPath,
                    Link);

                await Task.Run(() =>
                {
                    Process process = Process.Start(mv.Model.SettingsViewModel.YouPath, param);
                    if (process != null)
                    {
                        process.Close();
                    }
                });
            }

            window.Close();
        }

        #endregion
    }
}
