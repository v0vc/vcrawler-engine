﻿// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Interfaces.Enums;
using Interfaces.Models;

namespace Models.BO.Items
{
    public class RuTrackerItem : IVideoItem
    {
        #region Methods

        public IVideoItem CreateVideoItem()
        {
            throw new NotImplementedException();
        }

        public Task DeleteItemAsync()
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

        public Task Log(string text)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVideoItem Members

        public string CommentCountText { get; }
        public long Comments { get; set; }
        public string DateTimeAgo { get; set; }
        public string Description { get; set; }
        public string DislikeCountText { get; }
        public double DownloadPercentage { get; set; }
        public int Duration { get; set; }
        public string DurationString { get; set; }
        public ItemState FileState { get; set; }
        public string ID { get; set; }
        public string LikeCountText { get; }
        public string LocalFilePath { get; set; }
        public string LogText { get; set; }
        public string ParentID { get; set; }
        public string ParentTitle { get; set; }
        public string ProxyUrl { get; set; }
        public ObservableCollection<ISubtitle> Subtitles { get; set; }
        public SyncState SyncState { get; set; }
        public IEnumerable<ITag> Tags { get; set; }
        public byte[] Thumbnail { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public long ViewCount { get; set; }
        public long ViewDiff { get; set; }
        public long LikeCount { get; set; }
        public long DislikeCount { get; set; }
        public WatchState WatchState { get; set; }

        public Task DownloadItem(string youPath, string dirPath, PlaylistMenuItem dtype)
        {
            throw new NotImplementedException();

            // var httpRequest = (HttpWebRequest)WebRequest.Create(MakeLink());
            // httpRequest.Method = WebRequestMethods.Http.Post;

            // httpRequest.Referer = string.Format("{0}={1}", topicUrl, ID);
            // httpRequest.CookieContainer = cookie;

            // // Include post data in the HTTP request
            // const string postData = "dummy=";
            // httpRequest.ContentLength = postData.Length;
            // httpRequest.ContentType = "application/x-www-form-urlencoded";

            // // Write the post data to the HTTP request
            // var requestWriter = new StreamWriter(httpRequest.GetRequestStream(), Encoding.ASCII);
            // requestWriter.Write(postData);
            // requestWriter.Close();

            // var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            // Stream httpResponseStream = httpResponse.GetResponseStream();

            // const int bufferSize = 1024;
            // var buffer = new byte[bufferSize];

            // // Read from response and write to file
            // var dir = new DirectoryInfo(Path.Combine(dirPath, ParentID));
            // if (!dir.Exists)
            // {
            // dir.Create();
            // }

            // {
            // string dpath = AviodTooLongFileName(Path.Combine(dir.FullName, MakeTorrentFileName()));
            // FileStream fileStream = File.Create(dpath);
            // int bytesRead;
            // while (httpResponseStream != null && (bytesRead = httpResponseStream.Read(buffer, 0, bufferSize)) != 0)
            // {
            // fileStream.Write(buffer, 0, bytesRead);
            // } // end while
            // var fn = new FileInfo(dpath);
            // if (fn.Exists)
            // {
            // ItemState = "LocalYes";
            // IsHasLocalFile = true;
            // LocalFilePath = fn.FullName;
            // }
            // else
            // {
            // ItemState = "LocalNo";
            // IsHasLocalFile = false;
            // }
            // }
        }

        public Task FillDescriptionAsync()
        {
            throw new NotImplementedException();
        }

        public void IsHasLocalFileFound(string dir)
        {
            throw new NotImplementedException();
        }

        public string MakeLink()
        {
            throw new NotImplementedException();
        }

        public void OpenInFolder(string parentDir)
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
