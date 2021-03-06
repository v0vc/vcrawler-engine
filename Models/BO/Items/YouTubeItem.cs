﻿// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Extensions;
using Interfaces.Enums;
using Interfaces.Models;
using Microsoft.WindowsAPICodePack.Taskbar;
using Models.Factories;

namespace Models.BO.Items
{
    public sealed class YouTubeItem : IVideoItem, INotifyPropertyChanged
    {
        #region Static and Readonly Fields

        private readonly CancellationTokenSource cancelToken;

        private readonly string[] exts = { "mp4", "mkv", "mp3", "webm" };

        #endregion

        #region Fields

        private long comments;

        private string description;
        private long dislikeCount;
        private double downloadPercentage;
        private PlaylistMenuItem downType;
        private ItemState fileState;
        private bool isProxyReady;
        private long likeCount;
        private string logText;
        private SyncState syncState;
        private TaskbarManager taskbar;
        private string tempname = string.Empty;
        private long viewCount;
        private WatchState watchState;
        private long viewDiff;

        #endregion

        #region Constructors

        public YouTubeItem()
        {
            cancelToken = new CancellationTokenSource();
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

        public void CancelDownload()
        {
            cancelToken.Cancel();
        }

        public async Task FillSubtitles()
        {
            if (Subtitles.Any())
            {
                return;
            }

            IEnumerable<ISubtitle> res = await VideoItemFactory.GetVideoItemSubtitlesAsync(ID).ConfigureAwait(true);
            Subtitles.Clear();
            res.ForEach(x => Subtitles.Add(x));
        }

        private void ErrorOccured()
        {
            DownloadPercentage = 0;
            taskbar.SetProgressState(TaskbarProgressBarState.NoProgress);
            FileState = ItemState.LocalNo;
        }

        private async Task Log(string text)
        {
            await Task.Run(() => LogText += text + Environment.NewLine).ConfigureAwait(false);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ProcessExited()
        {
            DownloadPercentage = 100;
            taskbar.SetProgressState(TaskbarProgressBarState.NoProgress);

            if (tempname == string.Empty)
            {
                ErrorOccured();
                return;
            }

            tempname = tempname.TrimStart('"').TrimEnd('"');

            var fn = new FileInfo(tempname);

            string cleartitle = Title.MakeValidFileName();

            if (cleartitle.Equals(Path.GetFileNameWithoutExtension(fn.Name), StringComparison.InvariantCulture))
            {
                // в имени нет запретных знаков
                if (fn.Exists)
                {
                    FileState = ItemState.LocalYes;
                    LocalFilePath = fn.FullName;
                }
                else
                {
                    FileState = ItemState.LocalNo;
                }
            }
            else
            {
                // есть всякие двоеточия - переименуем по алгоритму, отталкиваясь от Title
                if (fn.DirectoryName == null)
                {
                    return;
                }

                var fnn = new FileInfo(Path.Combine(fn.DirectoryName, cleartitle + Path.GetExtension(fn.Name)));
                if (CommonExtensions.RenameFile(fn, fnn))
                {
                    FileState = ItemState.LocalYes;
                    LocalFilePath = fnn.FullName;
                }
                else
                {
                    FileState = ItemState.LocalNo;
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IVideoItem Members

        public string CommentCountText => $"Comments: {Comments}";

        public long Comments
        {
            get
            {
                return comments;
            }
            set
            {
                if (value == comments)
                {
                    return;
                }
                comments = value;
                OnPropertyChanged();
            }
        }

        public string DateTimeAgo { get; set; }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                if (value == description)
                {
                    return;
                }
                description = value;
                OnPropertyChanged();
            }
        }

        public long DislikeCount
        {
            get
            {
                return dislikeCount;
            }
            set
            {
                if (value == dislikeCount)
                {
                    return;
                }
                dislikeCount = value;
                OnPropertyChanged();
            }
        }

        public string DislikeCountText => $"Dislikes: {DislikeCount}";

        public double DownloadPercentage
        {
            get
            {
                return downloadPercentage;
            }
            set
            {
                downloadPercentage = value;
                OnPropertyChanged();
            }
        }

        public int Duration { get; set; }
        public string DurationString { get; set; }

        public ItemState FileState
        {
            get
            {
                return fileState;
            }
            set
            {
                if (value == fileState)
                {
                    return;
                }
                fileState = value;
                OnPropertyChanged();
            }
        }

        public string ID { get; set; }

        public long LikeCount
        {
            get
            {
                return likeCount;
            }
            set
            {
                if (value == likeCount)
                {
                    return;
                }
                likeCount = value;
                OnPropertyChanged();
            }
        }

        public string LikeCountText => $"Likes: {LikeCount}";

        public string LocalFilePath { get; set; }

        public string LogText
        {
            get
            {
                return logText;
            }
            set
            {
                if (value == logText)
                {
                    return;
                }
                logText = value;
                OnPropertyChanged();
            }
        }

        public string ParentID { get; set; }
        public string ParentTitle { get; set; }
        public string ProxyUrl { get; set; }
        public ObservableCollection<ISubtitle> Subtitles { get; set; }

        public SyncState SyncState
        {
            get
            {
                return syncState;
            }
            set
            {
                if (value == syncState)
                {
                    return;
                }
                syncState = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<ITag> Tags { get; set; }

        public byte[] Thumbnail { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }

        public long ViewCount
        {
            get
            {
                return viewCount;
            }
            set
            {
                if (value == viewCount)
                {
                    return;
                }
                viewCount = value;
                OnPropertyChanged();
            }
        }

        public long ViewDiff
        {
            get
            {
                return viewDiff;
            }
            set
            {
                if (value == viewDiff)
                {
                    return;
                }
                viewDiff = value;
                OnPropertyChanged();
            }
        }

        public WatchState WatchState
        {
            get
            {
                return watchState;
            }
            set
            {
                if (value == watchState)
                {
                    return;
                }
                watchState = value;
                OnPropertyChanged();
            }
        }

        public async Task DownloadItem(string youPath, string dirPath, PlaylistMenuItem dtype)
        {
            downType = dtype;
            if (!string.IsNullOrEmpty(ProxyUrl))
            {
                // isProxyReady = CommonExtensions.IsValidUrl(ProxyUrl) && CommonExtensions.IsUrlExist(ProxyUrl);
                isProxyReady = true;
            }

            FileState = ItemState.Downloading;
            DirectoryInfo dir = ParentID != null ? new DirectoryInfo(Path.Combine(dirPath, ParentID)) : new DirectoryInfo(dirPath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            var options = "--no-check-certificate --console-title --no-call-home --no-cache-dir";

            if (isProxyReady)
            {
                options += " --proxy " + ProxyUrl;
            }

            string param;
            switch (dtype)
            {
                case PlaylistMenuItem.Audio:
                    param = $"--extract-audio -o \"{dir}\\%(title)s.%(ext)s\" \"{MakeLink()}\" --audio-format mp3 {options}";
                    break;
                case PlaylistMenuItem.DownloadHd:
                    param = $"-f bestvideo+bestaudio, -o \"{dir}\\%(title)s.%(ext)s\" \"{MakeLink()}\" {options}";
                    break;

                case PlaylistMenuItem.DownloadSubs:
                    param = $"-f best, -o \"{dir}\\%(title)s.%(ext)s\" \"{MakeLink()}\" {options} --all-subs";
                    break;
                case PlaylistMenuItem.Download:
                    param = $"-f best, -o \"{dir}\\%(title)s.%(ext)s\" \"{MakeLink()}\" {options}";
                    break;
                case PlaylistMenuItem.Video:
                    param = $"-f bestvideo, -o \"{dir}\\%(title)s.%(ext)s\" \"{MakeLink()}\" {options}";
                    break;
                case PlaylistMenuItem.DownloadSubsOnly:
                    param = $"-f best, -o \"{dir}\\%(title)s.%(ext)s\" \"{MakeLink()}\" {options} --all-subs --skip-download";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dtype), dtype, null);
            }

            if (Subtitles.Select(x => x.IsChecked).Contains(true) && dtype != PlaylistMenuItem.DownloadSubsOnly)
            {
                var sb = new StringBuilder();
                foreach (ISubtitle sub in Subtitles.Where(sub => sub.IsChecked))
                {
                    sb.Append(sub.Language).Append(',');
                }
                string res = sb.ToString().TrimEnd(',');

                string srt = res == "Auto" ? "--write-auto-sub" : $"--write-sub --sub-lang {res}";

                param = $"{param} {srt}";
            }

            var startInfo = new ProcessStartInfo(youPath, param)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                ErrorDialog = false,
                CreateNoWindow = true
            };

            taskbar = TaskbarManager.Instance;
            taskbar.SetProgressState(TaskbarProgressBarState.Normal);

            await Log("======").ConfigureAwait(false);
            await Task.Run(() =>
            {
                var proc = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

                proc.OutputDataReceived += EncodeOnOutputDataReceived;
                proc.ErrorDataReceived += EncodeOnErrorDataReceived;
                proc.Exited += EncodeOnProcessExited;

                proc.Start();
                proc.StandardInput.Close();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
            },
                cancelToken.Token).ConfigureAwait(false);
        }

        public async Task FillDescriptionAsync()
        {
            string res = await CommonFactory.CreateSqLiteDatabase().GetVideoItemDescriptionAsync(ID).ConfigureAwait(false);
            Description = res.WordWrap(100);
        }

        public void IsHasLocalFileFound(string dir)
        {
            if (string.IsNullOrEmpty(dir))
            {
                return;
            }

            foreach (string ext in exts)
            {
                string cleartitle = Title.MakeValidFileName();
                string pathvid = Path.Combine(dir, ParentID, $"{cleartitle}.{ext}");
                var fnvid = new FileInfo(pathvid);
                if (fnvid.Exists)
                {
                    FileState = ItemState.LocalYes;
                    LocalFilePath = fnvid.FullName;
                    break;
                }
                FileState = ItemState.LocalNo;
            }
        }

        public string MakeLink()
        {
            return $"https://www.youtube.com/watch?v={ID}";
        }

        public void OpenInFolder(string parentDir)
        {
            string argument = FileState != ItemState.LocalYes ? parentDir + "\"" : "/select, \"" + LocalFilePath + "\"";
            Process.Start("explorer.exe", argument);
        }

        public async Task RunItem(string mpcpath)
        {
            if (string.IsNullOrEmpty(mpcpath))
            {
                throw new Exception("Path to MPC not set");
            }

            if (FileState == ItemState.LocalYes)
            {
                if (string.IsNullOrEmpty(LocalFilePath))
                {
                    throw new Exception("Local File Path not set");
                }
                string param = $"\"{LocalFilePath}\" /play";
                await Task.Run(() => Process.Start(mpcpath, param)).ConfigureAwait(false);
            }
            else
            {
                string param = $"\"{MakeLink()}\" /play";
                await Task.Run(() => Process.Start(mpcpath, param)).ConfigureAwait(false);
            }
        }

        #endregion

        #region Event Handling

        private async void EncodeOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            await Log(e.Data).ConfigureAwait(false);
        }

        private async void EncodeOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (cancelToken.IsCancellationRequested)
            {
                var proc = sender as Process;
                if (proc != null)
                {
                    proc.OutputDataReceived -= EncodeOnOutputDataReceived;
                    proc.ErrorDataReceived -= EncodeOnErrorDataReceived;
                    proc.Exited -= EncodeOnProcessExited;
                    proc.Kill();
                    ProcessExited();
                    return;
                }
            }
            if (e.Data == null)
            {
                ProcessExited();
                return;
            }

            if (downType == PlaylistMenuItem.Audio && e.Data.StartsWith("[ffmpeg] Destination"))
            {
                var regex = new Regex(@"(\[ffmpeg\] Destination: )(.+?)(\.(mp3))(.+)?");
                Match match = regex.Match(e.Data);
                if (match.Success)
                {
                    tempname = regex.Replace(e.Data, "$2$3") + match.Groups[match.Groups.Count - 1].ToString().Split(' ')[0];
                }
            }
            else
            {
                if (e.Data.StartsWith("[download] Destination"))
                {
                    var regex = new Regex(@"(\[download\] Destination: )(.+?)(\.(mp4|m4a|webm|flv|mp3|mkv))(.+)?");
                    Match match = regex.Match(e.Data);
                    if (match.Success)
                    {
                        tempname = regex.Replace(e.Data, "$2$3") + match.Groups[match.Groups.Count - 1].ToString().Split(' ')[0];
                    }
                }

                if (e.Data.StartsWith("[ffmpeg] Merging formats into"))
                {
                    var regex = new Regex(@"(\[ffmpeg\] Merging formats into )(.+?)(\.(mp4|m4a|webm|flv|mp3|mkv))(.+)?");
                    Match match = regex.Match(e.Data);
                    if (match.Success)
                    {
                        tempname = regex.Replace(e.Data, "$2$3") + match.Groups[match.Groups.Count - 1].ToString().Split(' ')[0];
                    }
                }

                if (e.Data.EndsWith("has already been downloaded"))
                {
                    var regex = new Regex(@"(\[download\])(.+?)(\.(mp4|m4a|webm|flv|mp3|mkv))(.+)?");
                    Match match = regex.Match(e.Data);
                    if (match.Success)
                    {
                        tempname = regex.Replace(e.Data, "$2$3");
                    }
                }
            }

            await Log(e.Data).ConfigureAwait(false);

            DownloadPercentage = GetPercentFromYoudlOutput(e.Data);

            taskbar.SetProgressValue((int)DownloadPercentage, 100);
        }

        private async void EncodeOnProcessExited(object sender, EventArgs e)
        {
            string logdata = FileState == ItemState.LocalYes ? $"{Title} DOWNLOADED!" : $"ERROR DOWNLOADING: {ID}";
            await Log(logdata).ConfigureAwait(false);
            var proc = sender as Process;
            if (proc == null)
            {
                return;
            }
            proc.OutputDataReceived -= EncodeOnOutputDataReceived;
            proc.ErrorDataReceived -= EncodeOnErrorDataReceived;
            proc.Exited -= EncodeOnProcessExited;
        }

        #endregion
    }
}
