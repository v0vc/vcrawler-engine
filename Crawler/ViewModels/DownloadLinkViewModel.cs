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
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class DownloadLinkViewModel
    {
        #region Fields

        private RelayCommand downloadLinkCommand;

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
            if (string.IsNullOrEmpty(Link))
            {
                return;
            }

            if (!CommonExtensions.IsValidUrl(Link))
            {
                MessageBox.Show("Can't parse URL");
                return;
            }

            //if (string.IsNullOrEmpty(_youPath))
            //{
            //    MessageBox.Show("Please, select youtube-dl");
            //    return;
            //}

            //if (string.IsNullOrEmpty(DirPath))
            //{
            //    MessageBox.Show("Please, set download directory");
            //    return;
            //}

            //var regex = new Regex(CommonExtensions.YouRegex);
            //Match match = regex.Match(Link);
            //if (match.Success)
            //{
            //    string id = match.Groups[1].Value;
            //    IVideoItem vi = await _vf.GetVideoItemNetAsync(id, SiteType.YouTube);
            //    vi.ParentID = null;
            //    SelectedVideoItem = vi;

            //    SelectedChannel = ServiceChannels.First();
            //    ServiceChannels.First().AddNewItem(vi, true);
            //    await vi.DownloadItem(_youPath, DirPath, IsHd, IsAudio);
            //    vi.IsNewItem = true;
            //}
            //else
            //{
            //    string param = string.Format("-o {0}\\%(title)s.%(ext)s {1} --no-check-certificate -i --console-title", DirPath, Link);
            //    await Task.Run(() =>
            //    {
            //        Process process = Process.Start(_youPath, param);
            //        if (process != null)
            //        {
            //            process.Close();
            //        }
            //    });
            //}

            window.Close();
        }

        #endregion
    }
}
