using System;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface IVideoItem
    {
        string ID { get; set; }

        string ParentID { get; set; }

        string Title { get; set; }

        string Description { get; set; }

        long ViewCount { get; set; }

        int Duration { get; set; }

        string DurationString { get; set; }

        string DateTimeAgo { get; set; }

        int Comments { get; set; }

        byte[] Thumbnail { get; set; }

        byte[] LargeThumb { get; set; }

        DateTime Timestamp { get; set; }

        bool IsNewItem { get; set; }

        bool IsShowRow { get; set; }

        bool IsHasLocalFile { get; set; }

        double DownloadPercentage { get; set; }


        IVideoItem CreateVideoItem();

        Task<IVideoItem> GetVideoItemDbAsync();

        Task<IVideoItem> GetVideoItemNetAsync();

        Task<IChannel> GetParentChannelAsync();

        Task InsertItemAsync();

        Task DeleteItemAsync();

        Task RunItem(string mpcpath);

        Task DownloadItem(string youPath, string dirPath, bool isHd);

        //Task DownloadItem(string youPath, string dirPath, string fPath);

        void IsHasLocalFileFound(string dir);

        string LocalFilePath { get; set; }

        string ItemState { get; set; }
    }
}
