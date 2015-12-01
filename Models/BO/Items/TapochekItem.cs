// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Interfaces.Enums;
using Interfaces.Models;

namespace Models.BO.Items
{
    public class TapochekItem : IVideoItem
    {
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
        public bool IsSelected { get; set; }
        public byte[] LargeThumb { get; set; }
        public string LocalFilePath { get; set; }
        public string LogText { get; set; }
        public string ParentID { get; set; }
        public SiteType Site { get; set; }
        public ItemState State { get; set; }
        public string ProxyUrl { get; set; }
        public byte[] Thumbnail { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public ObservableCollection<ISubtitle> Subtitles { get; set; }
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
