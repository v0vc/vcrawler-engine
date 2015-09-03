using System;
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
using Interfaces.Models;
using Microsoft.WindowsAPICodePack.Taskbar;
using Models.Factories;

namespace Models.BO
{
    public sealed class VideoItem : INotifyPropertyChanged, IVideoItem
    {
        private readonly VideoItemFactory _vf;
        private readonly string[] _exts = {"mp4", "mkv"};
        private double _downloadPercentage;
        private int _duration;
        private bool _isHasLocalFile;
        private bool _isShowRow;
        private string _itemState;
        private byte[] _largeThumb;
        private string _logText;
        private string _tempname = string.Empty;
        private DateTime _timestamp;
        private TaskbarManager _taskbar;
        private string _description;

        private VideoItem()
        {
        }

        public VideoItem(VideoItemFactory vf)
        {
            _vf = vf;
        }

        public string LogText
        {
            get
            {
                return _logText;
            }
            set
            {
                _logText = value;
                OnPropertyChanged();
            }
        }

        public string ID { get; set; }

        public string ParentID { get; set; }

        public string Title { get; set; }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public long ViewCount { get; set; }

        public int Duration
        {
            get
            {
                return _duration;
            }
            set
            {
                _duration = value;
                DurationString = IntTostrTime(_duration);
            }
        }

        public string DurationString { get; set; }

        public string DateTimeAgo { get; set; }

        public int Comments { get; set; }

        public byte[] Thumbnail { get; set; }

        public byte[] LargeThumb
        {
            get
            {
                return _largeThumb;
            }
            set
            {
                _largeThumb = value;
                OnPropertyChanged();
            }
        }

        public DateTime Timestamp
        {
            get
            {
                return _timestamp;
            }
            set
            {
                _timestamp = value;
                DateTimeAgo = TimeAgo(_timestamp);
            }
        }

        public string LocalFilePath { get; set; }

        public bool IsNewItem { get; set; }

        public bool IsShowRow
        {
            get
            {
                return _isShowRow;
            }
            set
            {
                _isShowRow = value;
                OnPropertyChanged();
            }
        }

        public bool IsHasLocalFile
        {
            get
            {
                return _isHasLocalFile;
            }
            set
            {
                _isHasLocalFile = value;
                OnPropertyChanged();
            }
        }

        public string ItemState
        {
            get
            {
                return _itemState;
            }
            set
            {
                _itemState = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IChapter> VideoItemChapters { get; set; }

        public double DownloadPercentage
        {
            get
            {
                return _downloadPercentage;
            }
            set
            {
                _downloadPercentage = value;
                OnPropertyChanged();
            }
        }

        public IVideoItem CreateVideoItem()
        {
            // return ServiceLocator.VideoItemFactory.CreateVideoItem();
            return _vf.CreateVideoItem();
        }

        public async Task<IVideoItem> GetVideoItemDbAsync()
        {
            return await _vf.GetVideoItemDbAsync(ID);
        }

        public async Task<IVideoItem> GetVideoItemNetAsync()
        {
            return await _vf.GetVideoItemNetAsync(ID);
        }

        public async Task<IChannel> GetParentChannelAsync()
        {
            return await _vf.GetParentChannelAsync(ParentID);
        }

        public async Task FillChapters()
        {
            var res = await _vf.GetVideoItemChaptersAsync(ID);

            VideoItemChapters.Clear();

            foreach (var chapter in res)
            {
                VideoItemChapters.Add(chapter);
            }
        }

        public async Task FillDescriptionAsync()
        {
            await _vf.FillDescriptionAsync(this);
        }

        public async Task InsertItemAsync()
        {
            await _vf.InsertItemAsync(this);
        }

        public async Task DeleteItemAsync()
        {
            await _vf.DeleteItemAsync(ID);
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
                var param = string.Format("\"{0}\" /play", LocalFilePath);
                await Task.Run(() => Process.Start(mpcpath, param));
            }
            else
            {
                var param = string.Format("\"{0}\" /play", MakeLink());
                await Task.Run(() => Process.Start(mpcpath, param));
            }
        }

        public void IsHasLocalFileFound(string dir)
        {
            if (string.IsNullOrEmpty(dir))
            {
                return;
            }

            foreach (var ext in _exts)
            {
                var cleartitle = Title.MakeValidFileName();
                var pathvid = Path.Combine(dir, ParentID, string.Format("{0}.{1}", cleartitle, ext));
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

        public string MakeLink()
        {
            // TODO по типу площадки
            return string.Format("https://www.youtube.com/watch?v={0}", ID);
        }

        public async Task DownloadItem(string youPath, string dirPath, bool isHd)
        {
            ItemState = "Downloading";
            var dir = ParentID != null ? new DirectoryInfo(Path.Combine(dirPath, ParentID)) : new DirectoryInfo(dirPath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            const string options = "--no-check-certificate --console-title --no-call-home";
            var param = string.Format(isHd
                ? "-f bestvideo+bestaudio, -o {0}\\%(title)s.%(ext)s {1} {2}"
                : "-f best, -o {0}\\%(title)s.%(ext)s {1} {2}", dir, MakeLink(), options);

            if (VideoItemChapters.Select(x => x.IsChecked).Contains(true))
            {
                var sb = new StringBuilder();
                foreach (var chapter in VideoItemChapters.Where(chapter => chapter.IsChecked))
                {
                    sb.Append(chapter.Language).Append(',');
                }
                var res = sb.ToString().TrimEnd(',');

                var srt = res == "Auto"
                    ? "--write-srt --write-auto-sub"
                    : string.Format("--write-srt --srt-lang {0}", res);

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

            _taskbar = TaskbarManager.Instance;
            _taskbar.SetProgressState(TaskbarProgressBarState.Normal);

            await Task.Run(() =>
            {
                var proc = new Process {StartInfo = startInfo, EnableRaisingEvents = true};

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

        public async Task Log(string text)
        {
            await Task.Run(() => LogText += text + Environment.NewLine);
        }

        private async void EncodeOnProcessExited(object sender, EventArgs e)
        {
            await Log("FINISHED!");
            var proc = sender as Process;
            if (proc != null)
            {
                proc.OutputDataReceived -= EncodeOnOutputDataReceived;
                proc.ErrorDataReceived -= EncodeOnErrorDataReceived;
                proc.Exited -= EncodeOnProcessExited;
            }
        }

        private async void EncodeOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            await Log(e.Data);
        }

        private async void EncodeOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                Process_Exited();
                return;
            }

            if (e.Data.StartsWith("[download] Destination"))
            {
                var regex = new Regex(@"(\[download\] Destination: )(.+?)(\.(mp4|m4a|webm|flv|mp3|mkv))(.+)?");
                var match = regex.Match(e.Data);
                if (match.Success)
                {
                    _tempname = regex.Replace(e.Data, "$2$3") +
                                match.Groups[match.Groups.Count - 1].ToString().Split(' ')[0];
                }
            }

            if (e.Data.StartsWith("[ffmpeg] Merging formats into"))
            {
                var regex = new Regex(@"(\[ffmpeg\] Merging formats into )(.+?)(\.(mp4|m4a|webm|flv|mp3|mkv))(.+)?");
                var match = regex.Match(e.Data);
                if (match.Success)
                {
                    _tempname = regex.Replace(e.Data, "$2$3") +
                                match.Groups[match.Groups.Count - 1].ToString().Split(' ')[0];
                }
            }

            if (e.Data.EndsWith("has already been downloaded"))
            {
                var regex = new Regex(@"(\[download\])(.+?)(\.(mp4|m4a|webm|flv|mp3|mkv))(.+)?");
                var match = regex.Match(e.Data);
                if (match.Success)
                {
                    _tempname = regex.Replace(e.Data, "$2$3");
                }
            }

            await Log(e.Data);

            DownloadPercentage = GetPercentFromYoudlOutput(e.Data);
           
            _taskbar.SetProgressValue((int)DownloadPercentage, 100);
        }

        private static double GetPercentFromYoudlOutput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }
            var regex = new Regex(@"[0-9][0-9]{0,2}\.[0-9]%", RegexOptions.None);
            var match = regex.Match(input);
            if (!match.Success)
            {
                return 0;
            }
            double res;
            var str = match.Value.TrimEnd('%').Replace('.', ',');
            return double.TryParse(str, out res) ? res : 0;
        }

        private void Process_Exited()
        {
            DownloadPercentage = 100;
            _taskbar.SetProgressState(TaskbarProgressBarState.NoProgress);

            if (_tempname == string.Empty)
            {
                return;
            }

            _tempname = _tempname.TrimStart('"').TrimEnd('"');

            var fn = new FileInfo(_tempname);

            var cleartitle = Title.MakeValidFileName();


            if (cleartitle == fn.Name)
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

        private static string IntTostrTime(int duration)
        {
            var t = TimeSpan.FromSeconds(duration);
            return t.Hours > 0
                ? string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds)
                : string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        private static string TimeAgo(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.Days > 365)
            {
                var years = span.Days/365;
                if (span.Days%365 != 0)
                {
                    years += 1;
                }
                return string.Format("about {0} {1} ago", years, years == 1 ? "year" : "years");
            }
            if (span.Days > 30)
            {
                var months = span.Days/30;
                if (span.Days%31 != 0)
                {
                    months += 1;
                }
                return string.Format("about {0} {1} ago", months, months == 1 ? "month" : "months");
            }
            if (span.Days > 0)
            {
                return string.Format("about {0} {1} ago", span.Days, span.Days == 1 ? "day" : "days");
            }
            if (span.Hours > 0)
            {
                return string.Format("about {0} {1} ago", span.Hours, span.Hours == 1 ? "hour" : "hours");
            }
            if (span.Minutes > 0)
            {
                return string.Format("about {0} {1} ago", span.Minutes, span.Minutes == 1 ? "minute" : "minutes");
            }
            if (span.Seconds > 5)
            {
                return string.Format("about {0} seconds ago", span.Seconds);
            }
            if (span.Seconds <= 5)
            {
                return "just now";
            }
            return string.Empty;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}