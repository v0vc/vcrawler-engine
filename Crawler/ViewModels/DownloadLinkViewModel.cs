// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public sealed class DownloadLinkViewModel : INotifyPropertyChanged
    {
        #region Static and Readonly Fields

        private readonly string downloaddir;
        private readonly Action<IVideoItem> onDownloadYouItem;
        private readonly string youpath;

        #endregion

        #region Fields

        private RelayCommand downloadLinkCommand;
        private bool isYouTube;
        private string link;
        private RelayCommand subtitlesDropDownOpenedCommand;
        private string youId;

        #endregion

        #region Constructors

        public DownloadLinkViewModel(string youpath, string downloaddir, Action<IVideoItem> onDownloadYouItem = null)
        {
            this.youpath = youpath;
            this.downloaddir = downloaddir;
            this.onDownloadYouItem = onDownloadYouItem;
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

        public RelayCommand DownloadLinkCommand => downloadLinkCommand ?? (downloadLinkCommand = new RelayCommand(DownloadLink));

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
            =>
                subtitlesDropDownOpenedCommand
                ?? (subtitlesDropDownOpenedCommand = new RelayCommand(async x => await FillSubtitles().ConfigureAwait(false)));

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

            if (!Link.IsValidUrl())
            {
                MessageBox.Show("Can't parse URL");
                return;
            }

            if (IsYouTube)
            {
                IVideoItem vi = await VideoItemFactory.GetVideoItemNetAsync(youId, SiteType.YouTube).ConfigureAwait(true);
                foreach (ISubtitle subtitle in Subtitles.Where(subtitle => subtitle.IsChecked))
                {
                    vi.Subtitles.Add(subtitle);
                }
                vi.ParentID = null;
                vi.SyncState = SyncState.Added;
                onDownloadYouItem?.Invoke(vi);
                await vi.DownloadItem(youpath, downloaddir, IsHd, IsAudio).ConfigureAwait(false);
            }
            else
            {
                string param = $"-o {downloaddir}\\%(title)s.%(ext)s {Link} --no-check-certificate -i --console-title";

                await Task.Run(() =>
                {
                    Process process = Process.Start(youpath, param);
                    process?.Close();
                }).ConfigureAwait(false);
            }
        }

        private async Task FillSubtitles()
        {
            if (Subtitles.Any())
            {
                return;
            }

            List<SubtitlePOCO> res = await YouTubeSite.GetVideoSubtitlesByIdAsync(youId).ConfigureAwait(false);
            if (res.Any())
            {
                Subtitles.Clear();
                foreach (ISubtitle sb in res.Select(SubtitleFactory.CreateSubtitle))
                {
                    Subtitles.Add(sb);
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ParseYou(string text)
        {
            string id;
            if (CommonExtensions.IsLinkYoutube(text, out id))
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
