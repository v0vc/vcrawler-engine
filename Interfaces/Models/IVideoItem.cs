// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Interfaces.Enums;

namespace Interfaces.Models
{
    public interface IVideoItem
    {
        #region Properties

        int Comments { get; set; }
        string DateTimeAgo { get; set; }
        string Description { get; set; }
        double DownloadPercentage { get; set; }
        int Duration { get; set; }
        string DurationString { get; set; }
        ItemState FileState { get; set; }
        string ID { get; set; }
        bool IsHasLocalFile { get; set; }
        bool IsSelected { get; set; }
        byte[] LargeThumb { get; set; }
        string LocalFilePath { get; set; }
        string LogText { get; set; }
        string ParentID { get; set; }
        string ProxyUrl { get; set; }
        SiteType Site { get; set; }
        ObservableCollection<ISubtitle> Subtitles { get; set; }
        SyncState SyncState { get; set; }
        byte[] Thumbnail { get; set; }
        DateTime Timestamp { get; set; }
        string Title { get; set; }
        long ViewCount { get; set; }

        #endregion

        #region Methods

        Task DownloadItem(string youPath, string dirPath, bool isHd, bool isAudio);

        Task FillDescriptionAsync();

        Task InsertItemAsync();

        void IsHasLocalFileFound(string dir);

        Task Log(string text);

        string MakeLink();

        Task RunItem(string mpcpath);

        #endregion
    }
}
