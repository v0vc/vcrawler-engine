using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Extensions;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
//using Models.Extensions;
using Models.Factories;

namespace Models.BO
{
    public class VideoItem : INotifyPropertyChanged,IVideoItem
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private readonly VideoItemFactory _vf;

        private int _duration;

        private DateTime _timestamp;

        private bool _isShowRow;

        private double _downloadPercentage;

        private bool _isHasLocalFile;

        private string _itemState;

        private readonly List<string> _destList = new List<string>();

        private byte[] _largeThumb;

        private string _logText;

        public string ID { get; set; }

        public string ParentID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int ViewCount { get; set; }

        public int Duration
        {
            get { return _duration; }
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
            get { return _largeThumb; }
            set
            {
                _largeThumb = value; 
                OnPropertyChanged();
            }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
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
            get { return _isShowRow; }
            set
            {
                _isShowRow = value;
                OnPropertyChanged();
            }
        }

        public bool IsHasLocalFile
        {
            get { return _isHasLocalFile; }
            set
            {
                _isHasLocalFile = value;
                OnPropertyChanged();
            }
        }

        public string ItemState
        {
            get { return _itemState; }
            set
            {
                _itemState = value; 
                OnPropertyChanged();
            }
        }

        public double DownloadPercentage
        {
            get { return _downloadPercentage; }
            set
            {
                _downloadPercentage = value;
                OnPropertyChanged();
            }
        }

        public string LogText
        {
            get { return _logText; }
            set
            {
                _logText = value;
                OnPropertyChanged();
            }
        }


        public VideoItem(IVideoItemFactory vf)
        {
            _vf = vf as VideoItemFactory;
        }

        public VideoItem(IVideoItemPOCO item, IVideoItemFactory vf)
        {
            _vf = vf as VideoItemFactory;
            ID = item.ID;
            Title = item.Title;
            ParentID = item.ParentID;
            Description = item.Description.WordWrap(80);
            ViewCount = item.ViewCount;
            Duration = item.Duration;
            Comments = item.Comments;
            Thumbnail = item.Thumbnail;
            Timestamp = item.Timestamp;
        }

        public IVideoItem CreateVideoItem()
        {
            return _vf.CreateVideoItem();
            //return ServiceLocator.VideoItemFactory.CreateVideoItem();
        }

        public async Task<IVideoItem> GetVideoItemDbAsync()
        {
            return await _vf.GetVideoItemDbAsync(ID);
            //return await ServiceLocator.VideoItemFactory.GetVideoItemDbAsync(ID);
        }

        public async Task<IVideoItem> GetVideoItemNetAsync()
        {
            return await _vf.GetVideoItemNetAsync(ID);
            //return await ServiceLocator.VideoItemFactory.GetVideoItemNetAsync(ID);
        }

        public async Task<IChannel> GetParentChannelAsync()
        {
            return await _vf.GetParentChannelAsync(ParentID);
            //return await ((VideoItemFactory) ServiceLocator.VideoItemFactory).GetParentChannelAsync(ParentID);
        }

        public async Task InsertItemAsync()
        {
            await _vf.InsertItemAsync(this);
            //await ((VideoItemFactory) ServiceLocator.VideoItemFactory).InsertItemAsync(this);
        }

        public async Task DeleteItemAsync()
        {
            await _vf.DeleteItemAsync(ID);
            //await ((VideoItemFactory) ServiceLocator.VideoItemFactory).DeleteItemAsync(ID);
        }

        public async Task RunItem(string mpcpath)
        {
            if (string.IsNullOrEmpty(mpcpath))
                return;

            if (IsHasLocalFile)
            {
                if (string.IsNullOrEmpty(LocalFilePath)) return;
                var param = string.Format("\"{0}\" /play", LocalFilePath);
                await Task.Run(() => Process.Start(mpcpath, param));
            }
            else
            {
                var param = string.Format("\"{0}\" /play", MakeVideoLink());
                await Task.Run(() => Process.Start(mpcpath, param));
            }
        }

        public void IsHasLocalFileFound(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return;
            var pathvid = Path.Combine(dir, ParentID, string.Format("{0}.mp4", Title.MakeValidFileName()));
            var fnvid = new FileInfo(pathvid);
            IsHasLocalFile = fnvid.Exists;
            if (fnvid.Exists)
            {
                ItemState = "LocalYes";
                LocalFilePath = fnvid.FullName;
            }
            else
            {
                ItemState = "LocalNo";
            }
        }

        public async Task DownloadItem(string youPath, string dirPath, bool isHd)
        {
            ItemState = "Downloading";
            var dir = new DirectoryInfo(Path.Combine(dirPath, ParentID));
            if (!dir.Exists)
                dir.Create();
            string param;
            if (isHd)
            {
                param = String.Format("-f bestvideo+bestaudio, -o {0}\\%(title)s.%(ext)s {1} --no-check-certificate --console-title --restrict-filenames",
                            dir, MakeVideoLink());
            }
            else
            {
                param = String.Format("-f best, -o {0}\\%(title)s.%(ext)s {1} --no-check-certificate --console-title --restrict-filenames",
                            dir, MakeVideoLink());
            }

            var startInfo = new ProcessStartInfo(youPath, param)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            await Task.Run(() =>
            {
                var process = Process.Start(startInfo);
                if (process == null) return;
                process.OutputDataReceived += (sender, e) => SetLogAndPercentage(e.Data, youPath);
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
                process.Close();    
            });
        }

        private async void SetLogAndPercentage(string data, string youPath)
        {
            if (data == null)
            {
                processDownload_Exited(youPath);
                return;
            }

            await Log(data);

            DownloadPercentage = GetPercentFromYoudlOutput(data);

            var dest = GetDestinationFromYoudlOutput(data);
            if (!string.IsNullOrEmpty(dest))
                _destList.Add(dest);
        }

        private static double GetPercentFromYoudlOutput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;
            var regex = new Regex(@"[0-9][0-9]{0,2}\.[0-9]%", RegexOptions.None);
            var match = regex.Match(input);
            if (!match.Success) return 0;
            double res;
            var str = match.Value.TrimEnd('%').Replace('.', ',');
            return double.TryParse(str, out res) ? res : 0;
        }

        private static string GetDestinationFromYoudlOutput(string input)
        {
            var regex = new Regex(@"(\[download\] Destination: )(.+?)(\.(mp4|m4a|webm|flv|mp3))(.+)?");
            var match = regex.Match(input);
            if (match.Success)
            {
                return regex.Replace(input, "$2$3") + match.Groups[match.Groups.Count - 1].ToString().Split(' ')[0];
                //return regex.Replace(input, "$2$3"); //для обычных файлов
            }
            regex = new Regex(@"(\[download\])(.+?)(\.(mp4|m4a|webm|flv|mp3))(.+)?");
            match = regex.Match(input);
            if (match.Success)
            {
                return regex.Replace(input, "$2$3") + match.Groups[match.Groups.Count - 1].ToString().Split(' ')[0]; //для файлов, у которых в названии есть точка + расширение
                //return regex.Replace(input, "$2$3"); //для обычных файлов
            }
            return string.Empty;
        }

        private void processDownload_Exited(string youPath)
        {
            DownloadPercentage = 100;

            if (!_destList.Any()) return;

            if (_destList.Count == 1)
            {
                var fn = new FileInfo(_destList.First());

                if (fn.DirectoryName == null) return;

                var filename = CommonExtensions.GetVersion(youPath,
                    String.Format("--get-filename -o \"%(title)s.%(ext)s\" {0}", MakeVideoLink()));

                var fnn = new FileInfo(Path.Combine(fn.DirectoryName, filename.MakeValidFileName()));

                if (CommonExtensions.RenameFile(fn, fnn))
                {
                    ItemState = "LocalYes";
                    IsHasLocalFile = true;
                    LocalFilePath = fnn.FullName;
                }
                else
                    IsHasLocalFile = false;
            }

            if (_destList.Count == 2)
            {
                var vid = _destList.FirstOrDefault(x => x.EndsWith(".mp4") || x.EndsWith(".webm"));
                if (vid != null)
                {
                    var vidsp = vid.Split('.');
                    var vidname = vidsp.Take(vidsp.Length - 2);
                    var sb = new StringBuilder();
                    foreach (string s in vidname)
                    {
                        sb.Append(s).Append('.');
                    }
                    var name = sb + vidsp.Last();

                    var fn = new FileInfo(name);
                    if (fn.Exists && fn.DirectoryName != null)
                    {
                        var filename = CommonExtensions.GetVersion(youPath,
                            String.Format("--get-filename -o \"%(title)s.%(ext)s\" {0}", MakeVideoLink()));
                        var fnn = new FileInfo(Path.Combine(fn.DirectoryName, filename.MakeValidFileName()));

                        if (CommonExtensions.RenameFile(fn, fnn))
                        {
                            ItemState = "LocalYes";
                            IsHasLocalFile = true;
                            LocalFilePath = fnn.FullName;
                        }
                        else
                            IsHasLocalFile = false;
                    }
                }
            }

            _destList.Clear();
        }

        internal string IntTostrTime(int duration)
        {
            TimeSpan t = TimeSpan.FromSeconds(duration);
            return t.Hours > 0 ? string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds) : string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        internal static string TimeAgo(DateTime dt)
        {
            TimeSpan span = DateTime.Now - dt;
            if (span.Days > 365)
            {
                int years = (span.Days / 365);
                if (span.Days % 365 != 0)
                    years += 1;
                return String.Format("about {0} {1} ago",
                years, years == 1 ? "year" : "years");
            }
            if (span.Days > 30)
            {
                int months = (span.Days / 30);
                if (span.Days % 31 != 0)
                    months += 1;
                return String.Format("about {0} {1} ago",
                months, months == 1 ? "month" : "months");
            }
            if (span.Days > 0)
                return String.Format("about {0} {1} ago",
                span.Days, span.Days == 1 ? "day" : "days");
            if (span.Hours > 0)
                return String.Format("about {0} {1} ago",
                span.Hours, span.Hours == 1 ? "hour" : "hours");
            if (span.Minutes > 0)
                return String.Format("about {0} {1} ago",
                span.Minutes, span.Minutes == 1 ? "minute" : "minutes");
            if (span.Seconds > 5)
                return String.Format("about {0} seconds ago", span.Seconds);
            if (span.Seconds <= 5)
                return "just now";
            return string.Empty;
        }

        internal string MakeVideoLink()
        {
            return string.Format("https://www.youtube.com/watch?v={0}", ID);
        }

        public async Task Log(string text)
        {
            await Task.Run(() => LogText += (text + Environment.NewLine));
        }
    }
}
