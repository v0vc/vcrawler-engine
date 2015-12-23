// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Crawler.Common;
using DataAPI.POCO;
using DataAPI.Videos;
using Extensions;
using Interfaces.Enums;
using Interfaces.Models;
using Models.Factories;

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
        private RelayCommand subtitlesDropDownOpenedCommand;
        private string youId;

        #endregion

        #region Constructors

        public DownloadLinkViewModel(MainWindowViewModel mv)
        {
            this.mv = mv;
            Subtitles = new ObservableCollection<ISubtitle>();
            var text = Clipboard.GetData(DataFormats.Text) as string;
            if (string.IsNullOrWhiteSpace(text) || text.Contains(Environment.NewLine))
            {
                text = text.RemoveSpecialCharacters();
            }
            Link = text;
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

        public ObservableCollection<ISubtitle> Subtitles { get; set; }

        public RelayCommand SubtitlesDropDownOpenedCommand
        {
            get
            {
                return subtitlesDropDownOpenedCommand
                       ?? (subtitlesDropDownOpenedCommand = new RelayCommand(async x => await FillSubtitles()));
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

            if (!Link.IsValidUrl())
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
                IVideoItem vi = await CommonFactory.CreateVideoItemFactory().GetVideoItemNetAsync(youId, SiteType.YouTube);
                foreach (ISubtitle subtitle in Subtitles.Where(subtitle => subtitle.IsChecked))
                {
                    vi.Subtitles.Add(subtitle);
                }
                vi.ParentID = null;
                mv.SelectedChannel = mv.ServiceChannel;
                mv.SelectedChannel.SelectedItem = vi;
                mv.SelectedChannel.AddNewItem(vi);
                await vi.DownloadItem(mv.SettingsViewModel.YouPath, mv.SettingsViewModel.DirPath, IsHd, IsAudio);
                vi.SyncState = SyncState.Added;
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

        private async Task FillSubtitles()
        {
            if (Subtitles.Any())
            {
                return;
            }

            var cf = CommonFactory.CreateSubtitleFactory();
            IEnumerable<SubtitlePOCO> res = (await YouTubeSite.GetVideoSubtitlesByIdAsync(youId)).ToList();
            if (res.Any())
            {
                Subtitles.Clear();
                foreach (ISubtitle sb in res.Select(poco => cf.CreateSubtitle(poco)))
                {
                    Subtitles.Add(sb);
                }
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
