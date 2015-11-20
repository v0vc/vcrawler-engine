// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Crawler.Common;
using Extensions;
using Interfaces.Enums;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class DownloadLinkViewModel
    {
        #region Static and Readonly Fields

        private readonly MainWindowViewModel mv;

        #endregion

        #region Fields

        private RelayCommand downloadLinkCommand;

        #endregion

        #region Constructors

        public DownloadLinkViewModel(MainWindowViewModel mv)
        {
            this.mv = mv;
            var text = Clipboard.GetData(DataFormats.Text) as string;
            if (string.IsNullOrWhiteSpace(text) || text.Contains(Environment.NewLine))
            {
                Link = CommonExtensions.RemoveSpecialCharacters(text);
            }
            else
            {
                Link = text;
            }
        }

        #endregion

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
            window.Close();

            if (string.IsNullOrEmpty(Link))
            {
                return;
            }

            if (!CommonExtensions.IsValidUrl(Link))
            {
                MessageBox.Show("Can't parse URL");
                return;
            }

            if (!mv.IsYoutubeExist())
            {
                return;
            }

            var regex = new Regex(CommonExtensions.YouRegex);
            Match match = regex.Match(Link);
            if (match.Success)
            {
                string id = match.Groups[1].Value;
                IVideoItem vi = await mv.BaseFactory.CreateVideoItemFactory().GetVideoItemNetAsync(id, SiteType.YouTube);
                vi.ParentID = null;
                mv.SelectedVideoItem = vi;
                mv.SelectedChannel = mv.ServiceChannel;
                mv.SelectedChannel.AddNewItem(vi, true);

                await vi.DownloadItem(mv.SettingsViewModel.YouPath, mv.SettingsViewModel.DirPath, IsHd, IsAudio);
                vi.IsNewItem = true;
            }
            else
            {
                string param = string.Format("-o {0}\\%(title)s.%(ext)s {1} --no-check-certificate -i --console-title", 
                    mv.SettingsViewModel.DirPath, 
                    Link);

                await Task.Run(() =>
                {
                    Process process = Process.Start(mv.SettingsViewModel.YouPath, param);
                    if (process != null)
                    {
                        process.Close();
                    }
                });
            }
        }

        #endregion
    }
}
