// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Extensions;
using Interfaces;
using Interfaces.Enums;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class DownloadLinkViewModel : INotifyPropertyChanged
    {
        #region Static and Readonly Fields

        private readonly MainWindowViewModel mv;

        #endregion

        #region Fields

        private RelayCommand downloadLinkCommand;
        private bool isYouTube;
        private string link;
        private string youId;

        #endregion

        #region Constructors

        public DownloadLinkViewModel(MainWindowViewModel mv)
        {
            this.mv = mv;
            var text = Clipboard.GetData(DataFormats.Text) as string;
            if (string.IsNullOrWhiteSpace(text) || text.Contains(Environment.NewLine))
            {
                text = CommonExtensions.RemoveSpecialCharacters(text);
            }
            ParseYou(text);
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

        public bool IsYouTube
        {
            get
            {
                return isYouTube;
            }
            private set
            {
                if (value == isYouTube)
                {
                    return;
                }
                isYouTube = value;
                OnPropertyChanged();
            }
        }

        public string Link
        {
            get
            {
                return link;
            }
            set
            {
                if (value == link)
                {
                    return;
                }
                link = value;
                ParseYou(link);
                OnPropertyChanged();
            }
        }

        #endregion

        #region Static Methods

        private static bool IsLinkYoutube(string text, out string videoId)
        {
            videoId = string.Empty;
            var regex = new Regex(CommonExtensions.YouRegex);
            Match match = regex.Match(text);
            if (match.Success)
            {
                videoId = match.Groups[1].Value;
                return true;
            }
            return false;
        }

        #endregion

        #region Methods

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

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

            if (IsYouTube)
            {
                IVideoItem vi = await mv.BaseFactory.CreateVideoItemFactory().GetVideoItemNetAsync(youId, SiteType.YouTube);
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

        private void ParseYou(string text)
        {
            string id;
            if (IsLinkYoutube(text, out id))
            {
                IsYouTube = true;
                youId = id;
            }
            else
            {
                IsYouTube = false;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
