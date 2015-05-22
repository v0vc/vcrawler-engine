using System;
using System.Threading.Tasks;
using System.Windows;

namespace Interfaces.Models
{
    public interface IVideoItem
    {
        string ID { get; set; }

        string ParentID { get; set; }

        string Title { get; set; }

        string Description { get; set; }

        int ViewCount { get; set; }

        int Duration { get; set; }

        string DurationString { get; set; }

        string DateTimeAgo { get; set; }

        int Comments { get; set; }

        byte[] Thumbnail { get; set; }

        DateTime Timestamp { get; set; }

        bool IsNewItem { get; set; }

        bool IsShowRow { get; set; }

        bool IsHasLocalFile { get; set; }

        double DownloadPercentage { get; set; }

        //Visibility ProgressBarVisibility { get; set; }


        IVideoItem CreateVideoItem();

        Task<IVideoItem> GetVideoItemDbAsync();

        Task<IVideoItem> GetVideoItemNetAsync();

        Task<IChannel> GetParentChannelAsync();

        Task InsertItemAsync();

        Task DeleteItemAsync();

        void RunItem(string mpcpath);

        void IsHasLocalFileFound(string dir);

        string LocalFilePath { get; set; }

        
    }
}
