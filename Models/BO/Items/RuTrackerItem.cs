// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Enums;
using Interfaces.Models;

namespace Models.BO.Items
{
    public class RuTrackerItem : IVideoItem
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
            //    var httpRequest = (HttpWebRequest)WebRequest.Create(MakeLink());
            //    httpRequest.Method = WebRequestMethods.Http.Post;

            //    httpRequest.Referer = string.Format("{0}={1}", topicUrl, ID);
            //    httpRequest.CookieContainer = cookie;

            //    // Include post data in the HTTP request
            //    const string postData = "dummy=";
            //    httpRequest.ContentLength = postData.Length;
            //    httpRequest.ContentType = "application/x-www-form-urlencoded";

            //    // Write the post data to the HTTP request
            //    var requestWriter = new StreamWriter(httpRequest.GetRequestStream(), Encoding.ASCII);
            //    requestWriter.Write(postData);
            //    requestWriter.Close();

            //    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            //    Stream httpResponseStream = httpResponse.GetResponseStream();

            //    const int bufferSize = 1024;
            //    var buffer = new byte[bufferSize];

            //    // Read from response and write to file
            //    var dir = new DirectoryInfo(Path.Combine(dirPath, ParentID));
            //    if (!dir.Exists)
            //    {
            //        dir.Create();
            //    }

            //    {
            //        string dpath = AviodTooLongFileName(Path.Combine(dir.FullName, MakeTorrentFileName()));
            //        FileStream fileStream = File.Create(dpath);
            //        int bytesRead;
            //        while (httpResponseStream != null && (bytesRead = httpResponseStream.Read(buffer, 0, bufferSize)) != 0)
            //        {
            //            fileStream.Write(buffer, 0, bytesRead);
            //        } // end while
            //        var fn = new FileInfo(dpath);
            //        if (fn.Exists)
            //        {
            //            ItemState = "LocalYes";
            //            IsHasLocalFile = true;
            //            LocalFilePath = fn.FullName;
            //        }
            //        else
            //        {
            //            ItemState = "LocalNo";
            //            IsHasLocalFile = false;
            //        }
            //    }
        }

        public Task FillSubtitles()
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

        public RelayCommand FillSubitlesCommand { get; private set; }

        #endregion
    }
}
