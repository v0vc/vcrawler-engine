// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
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
        string LocalFilePath { get; set; }
        string LogText { get; set; }
        string ParentID { get; set; }
        string ParentTitle { get; set; }
        string ProxyUrl { get; set; }
        ObservableCollection<ISubtitle> Subtitles { get; set; }
        SyncState SyncState { get; set; }
        IEnumerable<ITag> Tags { get; set; }
        byte[] Thumbnail { get; set; }
        DateTime Timestamp { get; set; }
        string Title { get; set; }
        long ViewCount { get; set; }
        WatchState WatchState { get; set; }

        #endregion

        #region Methods

        Task DownloadItem(string youPath, string dirPath, PlaylistMenuItem dtype);

        Task FillDescriptionAsync();

        void IsHasLocalFileFound(string dir);

        string MakeLink();

        void OpenInFolder(string parentDir);

        Task RunItem(string mpcpath);

        #endregion
    }
}
