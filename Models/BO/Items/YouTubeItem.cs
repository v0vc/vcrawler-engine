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

        private readonly string[] exts = { "mp4", "mkv", "mp3" };
        private readonly VideoItemFactory vf;

        #endregion

        #region Fields

        private double downloadPercentage;

        private bool isAudio;
        private bool isHasLocalFile;
        private bool isShowRow;
        private string itemState;
        private byte[] largeThumb;
        private string logText;
        private TaskbarManager taskbar;
        private string tempname = string.Empty;

        #endregion

        #region Constructors

        public YouTubeItem(VideoItemFactory vf)
        {
            this.vf = vf;
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

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ProcessExited()
        {
            DownloadPercentage = 100;
            taskbar.SetProgressState(TaskbarProgressBarState.NoProgress);

            if (tempname == string.Empty)
            {
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
                    ItemState = "LocalYes";
                    IsHasLocalFile = true;
                    LocalFilePath = fn.FullName;
                }
                else
                {
                    ItemState = "LocalNo";
                    IsHasLocalFile = false;
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
                    ItemState = "LocalYes";
                    IsHasLocalFile = true;
                    LocalFilePath = fnn.FullName;
                }
                else
                {
                    ItemState = "LocalNo";
                    IsHasLocalFile = false;
                }
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
        public string ID { get; set; }

        public bool IsHasLocalFile
        {
            get
            {
                return isHasLocalFile;
            }
            set
            {
                if (value == isHasLocalFile)
                {
                    return;
                }
                isHasLocalFile = value;
                OnPropertyChanged();
            }
        }

        public bool IsNewItem { get; set; }

        public bool IsShowRow
        {
            get
            {
                return isShowRow;
            }
            set
            {
                if (value == isShowRow)
                {
                    return;
                }
                isShowRow = value;
                OnPropertyChanged();
            }
        }

        public string ItemState
        {
            get
            {
                return itemState;
            }
            set
            {
                if (value == itemState)
                {
                    return;
                }
                itemState = value;
                OnPropertyChanged();
            }
        }

        public byte[] LargeThumb
        {
            get
            {
                return largeThumb;
            }
            set
            {
                if (value == largeThumb)
                {
                    return;
                }
                largeThumb = value;
                OnPropertyChanged();
            }
        }

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
        public SiteType Site { get; set; }
        public byte[] Thumbnail { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public ObservableCollection<IChapter> VideoItemChapters { get; set; }
        public long ViewCount { get; set; }

        public async Task DeleteItemAsync()
        {
            await vf.DeleteItemAsync(ID);
        }

        public async Task DownloadItem(string youPath, string dirPath, bool isHd, bool isAudiOnly)
        {
            isAudio = isAudiOnly;
            ItemState = "Downloading";
            DirectoryInfo dir = ParentID != null ? new DirectoryInfo(Path.Combine(dirPath, ParentID)) : new DirectoryInfo(dirPath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            const string options = "--no-check-certificate --console-title --no-call-home";

            string param;
            if (isAudio)
            {
                param = string.Format("--extract-audio -o {0}\\%(title)s.%(ext)s {1} --audio-format mp3 {2}", dir, MakeLink(), options);
            }
            else
            {
                param =
                    string.Format(
                                  isHd
                                      ? "-f bestvideo+bestaudio, -o {0}\\%(title)s.%(ext)s {1} {2}"
                                      : "-f best, -o {0}\\%(title)s.%(ext)s {1} {2}",
                        dir,
                        MakeLink(),
                        options);
            }

            if (VideoItemChapters.Select(x => x.IsChecked).Contains(true))
            {
                var sb = new StringBuilder();
                foreach (IChapter chapter in VideoItemChapters.Where(chapter => chapter.IsChecked))
                {
                    sb.Append(chapter.Language).Append(',');
                }
                string res = sb.ToString().TrimEnd(',');

                string srt = res == "Auto" ? "--write-srt --write-auto-sub" : string.Format("--write-srt --srt-lang {0}", res);

                param = string.Format("{0} {1}", param, srt);
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
            });
        }

        public async Task FillChapters()
        {
            IEnumerable<IChapter> res = await vf.GetVideoItemChaptersAsync(ID);

            VideoItemChapters.Clear();

            foreach (IChapter chapter in res)
            {
                VideoItemChapters.Add(chapter);
            }
        }

        public async Task FillDescriptionAsync()
        {
            await vf.FillDescriptionAsync(this);
        }

        public async Task<IChannel> GetParentChannelAsync()
        {
            return await vf.GetParentChannelAsync(ParentID);
        }

        public async Task<IVideoItem> GetVideoItemDbAsync()
        {
            return await vf.GetVideoItemDbAsync(ID);
        }

        public async Task<IVideoItem> GetVideoItemNetAsync()
        {
            return await vf.GetVideoItemNetAsync(ID, Site);
        }

        public async Task InsertItemAsync()
        {
            await vf.InsertItemAsync(this);
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
                string pathvid = Path.Combine(dir, ParentID, string.Format("{0}.{1}", cleartitle, ext));
                var fnvid = new FileInfo(pathvid);
                IsHasLocalFile = fnvid.Exists;
                if (IsHasLocalFile)
                {
                    ItemState = "LocalYes";
                    LocalFilePath = fnvid.FullName;
                    break;
                }
                ItemState = "LocalNo";
            }
        }

        public async Task Log(string text)
        {
            await Task.Run(() => LogText += text + Environment.NewLine);
        }

        public string MakeLink()
        {
            return string.Format("https://www.youtube.com/watch?v={0}", ID);
        }

        public async Task RunItem(string mpcpath)
        {
            if (string.IsNullOrEmpty(mpcpath))
            {
                throw new Exception("Path to MPC not set");
            }

            if (IsHasLocalFile)
            {
                if (string.IsNullOrEmpty(LocalFilePath))
                {
                    throw new Exception("Local File Path not set");
                }
                string param = string.Format("\"{0}\" /play", LocalFilePath);
                await Task.Run(() => Process.Start(mpcpath, param));
            }
            else
            {
                string param = string.Format("\"{0}\" /play", MakeLink());
                await Task.Run(() => Process.Start(mpcpath, param));
            }
        }

        #endregion

        #region Event Handling

        private async void EncodeOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            await Log(e.Data);
        }

        private async void EncodeOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                ProcessExited();
                return;
            }

            if (isAudio && e.Data.StartsWith("[ffmpeg] Destination"))
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

            await Log(e.Data);

            DownloadPercentage = GetPercentFromYoudlOutput(e.Data);

            taskbar.SetProgressValue((int)DownloadPercentage, 100);
        }

        private async void EncodeOnProcessExited(object sender, EventArgs e)
        {
            await Log("FINISHED!");
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