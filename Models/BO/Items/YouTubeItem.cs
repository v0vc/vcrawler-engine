// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Interfaces.Enums;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO.Items
{
    public class YouTubeItem : CommonItem, IVideoItem, INotifyPropertyChanged
    {
        #region Static and Readonly Fields

        private readonly string[] _exts = { "mp4", "mkv", "mp3" };
        private readonly VideoItemFactory _vf;

        #endregion

        #region Constructors

        public YouTubeItem(VideoItemFactory vf)
        {
            _vf = vf;
        }

        private YouTubeItem()
        {
        }

        #endregion

        #region Static Methods

        private static double GetPercentFromYoudlOutput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }
            var regex = new Regex(@"[0-9][0-9]{0,2}\.[0-9]%", RegexOptions.None);
            Match match = regex.Match(input);
            if (!match.Success)
            {
                return 0;
            }
            double res;
            string str = match.Value.TrimEnd('%').Replace('.', ',');
            return double.TryParse(str, out res) ? res : 0;
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

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IVideoItem Members

        public int Comments { get; set; }
        public string DateTimeAgo { get; set; }
        public string Description { get; set; }
        public double DownloadPercentage { get; set; }
        public int Duration { get; set; }
        public string DurationString { get; set; }
        public string ID { get; set; }
        public bool IsHasLocalFile { get; set; }
        public bool IsNewItem { get; set; }
        public bool IsShowRow { get; set; }
        public string ItemState { get; set; }
        public byte[] LargeThumb { get; set; }
        public string LocalFilePath { get; set; }
        public string LogText { get; set; }
        public string ParentID { get; set; }
        public SiteType Site { get; set; }
        public byte[] Thumbnail { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public ObservableCollection<IChapter> VideoItemChapters { get; set; }
        public long ViewCount { get; set; }

        public IVideoItem CreateVideoItem()
        {
            throw new NotImplementedException();
        }

        public Task DeleteItemAsync()
        {
            throw new NotImplementedException();
        }

        public Task DownloadItem(string youPath, string dirPath, bool isHd, bool isAudio)
        {
            throw new NotImplementedException();
        }

        public Task FillChapters()
        {
            throw new NotImplementedException();
        }

        public Task FillDescriptionAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IChannel> GetParentChannelAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IVideoItem> GetVideoItemDbAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IVideoItem> GetVideoItemNetAsync()
        {
            throw new NotImplementedException();
        }

        public Task InsertItemAsync()
        {
            throw new NotImplementedException();
        }

        public void IsHasLocalFileFound(string dir)
        {
            throw new NotImplementedException();
        }

        public Task Log(string text)
        {
            throw new NotImplementedException();
        }

        public string MakeLink()
        {
            throw new NotImplementedException();
        }

        public Task RunItem(string mpcpath)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
