using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Extensions;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.Extensions;
using Models.Factories;

namespace Models.BO
{
    public class VideoItem : INotifyPropertyChanged,IVideoItem
    {
        #region 

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

        //private Visibility _progressBarVisibility;
        private bool _isHasLocalFile;

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

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set
            {
                _timestamp = value;
                DateTimeAgo = TimeAgo(_timestamp);
            }
        }

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

        public double DownloadPercentage
        {
            get { return _downloadPercentage; }
            set
            {
                _downloadPercentage = value;
                OnPropertyChanged();
            }
        }

        //public Visibility ProgressBarVisibility
        //{
        //    get { return _progressBarVisibility; }
        //    set
        //    {
        //        _progressBarVisibility = value; 
        //        OnPropertyChanged();
        //    }
        //}

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
            Description = item.Description.WordWrap();
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

        public void RunItem(string mpcpath)
        {
            if (string.IsNullOrEmpty(mpcpath))
                return;

            if (IsHasLocalFile)
            {
                if (string.IsNullOrEmpty(LocalFilePath)) return;
                var param = string.Format("\"{0}\" /play", LocalFilePath);
                Process.Start(mpcpath, param);
            }
            else
            {
                var param = string.Format("\"{0}\" /play", string.Format("https://www.youtube.com/watch?v={0}", ID));
                Process.Start(mpcpath, param);
            }
        }

        public void IsHasLocalFileFound(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return;
            var pathvid = Path.Combine(dir, ParentID, string.Format("{0}.mp4", Title.MakeValidFileName()));
            var fnvid = new FileInfo(pathvid);
            IsHasLocalFile = fnvid.Exists;
            if (IsHasLocalFile)
                LocalFilePath = fnvid.FullName;
        }

        public string LocalFilePath { get; set; }

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


    }
}
